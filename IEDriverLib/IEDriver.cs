// Modified version of http://www.codeproject.com/Articles/9683/Automating-Internet-Explorer by Leslie Hanks

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using mshtml;
using SHDocVw;
using System.Runtime.InteropServices;

namespace IEDriverLib {
	/// <summary>
	/// Summary description for IEDriver.
	/// </summary>
	public class IEDriver {
		public int TimeoutSeconds {
			get { return m_timeoutSeconds; }
			set { m_timeoutSeconds = value; }
		}

		private InternetExplorer IE {
			get { return m_IE; }
		}

		private bool isDocumentComplete = false;

        private const int TIMES_TO_RETEST = 50;
        private ShellWindows m_windows = null;
		private InternetExplorer m_IE;
		private int m_timeoutSeconds = 60;
        private ManualResetEvent m_waitForRegister = null;
        private Process m_Proc = null;

        public IEDriver() {
            m_IE = new InternetExplorerClass();
            IE.Visible = false;
            IE.DocumentComplete += new DWebBrowserEvents2_DocumentCompleteEventHandler(IE_DocumentComplete);
        }

        private void windows_WindowRegistered(int lCookie) {
            if (m_Proc == null)
                return;  // This wasn't our window for sure

            for (int i = 0; i < m_windows.Count; i++) {
                InternetExplorer ShellWindow = m_windows.Item(i) as InternetExplorer;
                if (null != ShellWindow && IsUrl(((IWebBrowser)ShellWindow).LocationURL)) {
                    if ((IntPtr)ShellWindow.HWND == m_Proc.MainWindowHandle) {
                        m_IE = ShellWindow;
                        m_waitForRegister.Set(); // Signal the constructor that it is safe to go on now.
                        return;
                    }
                }
            }
        }

        private bool IsUrl(string sLocation)
        {
            if ("" == sLocation)
                return true; // workaround for an uninitialized window

            Uri uri = new Uri(sLocation);
            return uri.Port > 0?true:false;
        }

		public bool ClickAnchor(string anchorId) {
			HTMLAnchorElement input = GetAnchorElement(anchorId);
            if (null != input) {
                isDocumentComplete = false;
                input.click();
                return WaitForComplete();
            } else {
                return false;
            }
		}

		public bool ClickAnchorWithParent(string parentControlId, string anchorId) {
			anchorId = parentControlId + anchorId;
			HTMLAnchorElement input = GetAnchorElement(anchorId);
            if (null != input) {
                isDocumentComplete = false;
                input.click();
                return WaitForComplete();
            } else {
                return false;
            }
		}

		public bool ClickAnchorWithValue(string anchorValue) {
			HTMLAnchorElement anchor = (HTMLAnchorElement) GetElementByValue("A", anchorValue);
            if (null != anchor) {
                isDocumentComplete = false;
                anchor.click();
                return WaitForComplete();
            } else {
                return false;
            }
		}

        public bool ClickAnchorWithAttributeValue(string attrName, string attrValue) {
            HTMLAnchorElement anchor =
                (HTMLAnchorElement)GetElementByAttributeValue("A", attrName, attrValue);
            if (null != anchor) {
                isDocumentComplete = false;
                anchor.click();
                return WaitForComplete();
            } else {
                return false;
            }
        }

		public bool ClickInput(string buttonId) {
			HTMLInputElementClass input = GetInputElement(buttonId);
            if (null != input) {
                isDocumentComplete = false;
                input.click();
                return WaitForComplete();
            } else {
                return false;
            }
		}

        public bool ClickButton(string buttonId) {
            HTMLButtonElementClass input = GetButtonElement(buttonId);
            if (null != input) {
                isDocumentComplete = false;
                input.click();
                return WaitForComplete();
            } else {
                return false;
            }
        }

        public bool ClickButtonWithValue(string buttonValue) {
            HTMLInputElement input = (HTMLInputElement)GetInputElementByValue(buttonValue);
            if (null != input) {
                isDocumentComplete = false;
                input.click();
                return WaitForComplete();
            } else {
                return false;
            }
        }

        public bool ClickButtonWithInnerText(string innerText) {
            HTMLButtonElementClass input = (HTMLButtonElementClass)GetButtonElementByInnerText(innerText);
            if (null != input) {
                isDocumentComplete = false;
                input.click();
                return WaitForComplete();
            } else {
                return false;
            }
        }

		public void ClickCheckbox(string anchorId) {
			isDocumentComplete = false;
			HTMLInputElement input = GetCheckboxElement(anchorId);

			input.click();
		}

		public bool DoesElementExist(string elementId) {
			IHTMLElement input = GetElementById(elementId);
			return input != null;
		}

        public bool DoesElementWithAttributeValueExist(
            string tagName, string attrName, string attrValue) {
            IHTMLElement input = GetElementByAttributeValue(tagName, attrName, attrValue);
            return input != null;
        }

		public object GetElementAttribute(string elementId, string attributeName) {
			IHTMLElement element = GetElementById(elementId);
			if (element == null) {
				return null;
			}
			return element.getAttribute(attributeName, 0);
		}

		public IHTMLElement GetElementById(string elementId) {
            try {
                HTMLDocument document = ((HTMLDocument)IE.Document);
                IHTMLElement element = document.getElementById(elementId);

                int nullElementCount = 0;
                // The following loop is to account for any latency that IE
                // might experience.  Tweak the number of times to attempt
                // to continue checking the document before giving up.
                while (element == null && nullElementCount < TIMES_TO_RETEST) {
                    Thread.Sleep(500);
                    element = document.getElementById(elementId);
                    nullElementCount++;
                }

                return element;
            } catch {
                return null;
            }
		}

        public IHTMLElement GetElementByAttributeValue(string tagName, string attrName, string attrValue) {
            int nullElementCount = 0;
            IHTMLElement element = GetElementByAttributeValueOnce(tagName, attrName, attrValue);

            // The following loop is to account for any latency that IE
            // might experience.  Tweak the number of times to attempt
            // to continue checking the document before giving up.
            while (element == null && nullElementCount < TIMES_TO_RETEST) {
                Thread.Sleep(500);
                element = GetElementByAttributeValueOnce(tagName, attrName, attrValue);
                nullElementCount++;
            }

            return element;
        }

        protected IHTMLElement GetElementByAttributeValueOnce(string tagName, string attrName, string attrValue) {
            try {
                HTMLDocument document = ((HTMLDocument)IE.Document);
                IHTMLElementCollection tags = document.getElementsByTagName(tagName);

                IEnumerator enumerator = tags.GetEnumerator();

                while (enumerator.MoveNext()) {
                    IHTMLElement element = (IHTMLElement)enumerator.Current;
                    string attr = (string)element.getAttribute(attrName, 0);
                    if (attr == attrValue) {
                        return element;
                    }
                }

                return null;
            } catch {
                return null;
            }
        }

		public string GetInputValue(string inputId) {
			HTMLInputElementClass input = GetInputElement(inputId);

			if (input == null) {
				return null;
			}

			return input.value;
		}

		public bool Navigate(string url) {
			isDocumentComplete = false;
			object empty = "";
			IE.Navigate(url, ref empty, ref empty, ref empty, ref empty);
			return WaitForComplete();
		}

        public void Stop() {
            IE.Stop();
        }

        public void Quit() {
            IE.Quit();
        }

        public void Show() {
            IE.Visible = true;
        }

        public void Hide() {
            IE.Visible = false;
        }

		public void SetCheckboxValue(string checkboxId, bool isChecked, bool failIfNotExist) {
			HTMLInputElementClass input = GetInputElement(checkboxId);

			if (input == null && failIfNotExist) {
				throw new ApplicationException("CheckBox ID: " + checkboxId + " was not found.");
			}
			if (input != null) {
				input.@checked = isChecked;
			}
		}

		public void SetInputStringValue(string inputId, string elementValue) {
			HTMLInputElementClass input = GetInputElement(inputId);
			input.value = elementValue;
		}

		public void SetInputIntValue(string inputId, int elementValue) {
			HTMLInputElementClass input = GetInputElement(inputId);
			input.value = elementValue.ToString();
		}

        public void SetInputWithNameStringValue(string inputName, string elementValue) {
            HTMLInputElementClass input =
                (HTMLInputElementClass)GetElementByAttributeValue("input", "name", inputName);
            input.value = elementValue;
        }

		public void SelectValueByIndex(string inputId, int index) {
			HTMLSelectElementClass input = (HTMLSelectElementClass) GetSelectElement(inputId);
			input.selectedIndex = index;
		}

		protected IHTMLElement GetElementByValue(string tagName, string elementValue) {
			int nullElementCount = 0;
			IHTMLElement element = GetElementByValueOnce(tagName, elementValue);

			// The following loop is to account for any latency that IE
			// might experience.  Tweak the number of times to attempt
			// to continue checking the document before giving up.
            while (element == null && nullElementCount < TIMES_TO_RETEST) {
				Thread.Sleep(500);
				element = GetElementByValueOnce(tagName, elementValue);
				nullElementCount++;
			}

			return element;
		}

		private HTMLAnchorElement GetAnchorElement(string inputId) {
			return (HTMLAnchorElement) GetElementById(inputId);
		}

		private HTMLInputElement GetCheckboxElement(string inputId) {
			return (HTMLInputElement) GetElementById(inputId);
		}

		private IHTMLElement GetElementByValueOnce(string tagName, string elementValue) {
			HTMLDocument document = ((HTMLDocument) IE.Document);
			IHTMLElementCollection tags = document.getElementsByTagName(tagName);

			IEnumerator enumerator = tags.GetEnumerator();

			while (enumerator.MoveNext()) {
				IHTMLElement element = (IHTMLElement) enumerator.Current;
				if (element.innerText == elementValue) {
					return element;
				}
			}

			return null;
		}

		private HTMLInputElementClass GetInputElement(string inputId) {
			return (HTMLInputElementClass) GetElementById(inputId);
		}

        private HTMLButtonElementClass GetButtonElement(string inputId) {
            return (HTMLButtonElementClass)GetElementById(inputId);
        }

        private HTMLInputElementClass GetInputElementByValue(string elementValue) {
            int nullElementCount = 0;
            HTMLInputElementClass element = GetInputElementByValueOnce(elementValue);

            // The following loop is to account for any latency that IE
            // might experience.  Tweak the number of times to attempt
            // to continue checking the document before giving up.
            while (element == null && nullElementCount < TIMES_TO_RETEST) {
                Thread.Sleep(500);
                element = GetInputElementByValueOnce(elementValue);
                nullElementCount++;
            }

            return element;
        }

        private HTMLButtonElementClass GetButtonElementByInnerText(string elementInnerText) {
            int nullElementCount = 0;
            HTMLButtonElementClass element = GetButtonElementByInnerTextOnce(elementInnerText);

            // The following loop is to account for any latency that IE
            // might experience.  Tweak the number of times to attempt
            // to continue checking the document before giving up.
            while (element == null && nullElementCount < TIMES_TO_RETEST) {
                Thread.Sleep(500);
                element = GetButtonElementByInnerTextOnce(elementInnerText);
                nullElementCount++;
            }

            return element;
        }

        private HTMLInputElementClass GetInputElementByValueOnce(string elementValue) {
            HTMLDocument document = ((HTMLDocument)IE.Document);
            IHTMLElementCollection tags = document.getElementsByTagName("input");

            IEnumerator enumerator = tags.GetEnumerator();

            while (enumerator.MoveNext()) {
                HTMLInputElementClass element = (HTMLInputElementClass)enumerator.Current;
                if (element.value == elementValue) {
                    return element;
                }
            }

            return null;
        }

        private HTMLButtonElementClass GetButtonElementByInnerTextOnce(string elementValue) {
            HTMLDocument document = ((HTMLDocument)IE.Document);
            IHTMLElementCollection tags = document.getElementsByTagName("button");

            IEnumerator enumerator = tags.GetEnumerator();

            while (enumerator.MoveNext()) {
                HTMLButtonElementClass element = (HTMLButtonElementClass)enumerator.Current;
                if (element.innerText == elementValue) {
                    return element;
                }
            }

            return null;
        }

		private HTMLSelectElement GetSelectElement(string inputId) {
			return (HTMLSelectElement) GetElementById(inputId);
		}

		private bool WaitForComplete() {
			int elapsedSeconds = 0;
			while (!isDocumentComplete && elapsedSeconds != TimeoutSeconds) {
				Thread.Sleep(1000);
				elapsedSeconds++;
			}
            // check if timeout occurred
            return (isDocumentComplete && (elapsedSeconds < TimeoutSeconds))?true:false;
		}

		private void IE_DocumentComplete(object pDisp, ref object URL) {
			isDocumentComplete = true;
		}
	}
}