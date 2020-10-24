using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIToolbarControl : MonoBehaviour
    {
        Stack<Transform> ClickedButtons = new Stack<Transform>();

        void Start()
        {
            GetComponent<HorizontalLayoutGroup>().spacing = 0.0f;
            for (int i = 0; i < transform.childCount; ++i)
            {
                SetupTopbarMenu(i);
            }
        }

        void SetupTopbarMenu(int index)
        {
            var toolbarButton = transform.GetChild(index).GetComponent<Button>();
            AddListenerForToolbarButtonClicked(toolbarButton);

            var submenuVerticalGroup = toolbarButton.transform.GetChild(1);
            for (int i = 0; i < submenuVerticalGroup.childCount; ++i)
            {
                var submenuButton = submenuVerticalGroup.GetChild(i).GetComponent<Button>();
                if (submenuButton.name.Contains("Back"))
                {
                    AddListenerForBackButtonClicked(submenuButton);
                }
                else
                {
                    AddListenerForSubmenuButtonClickedRecursively(submenuButton);
                }
            }
            ToggleSubMenus(toolbarButton.transform, false);
        }

        void AddListenerForToolbarButtonClicked(Button button)
        {
            button?.onClick.AddListener(
                () =>
                {
                    if (ClickedButtons.Count == 0)
                    {
                        ClickedButtons.Push(button.transform);
                    }

                    if (ClickedButtons.Peek() != button.transform)
                    {
                        ClickedButtons.Push(button.transform);
                    }
                    ToggleSubMenus(ClickedButtons.Peek(), true);
                }
            );
        }

        void AddListenerForBackButtonClicked(Button button)
        {
            button?.onClick.AddListener(() =>
                {
                    ToggleSubMenus(ClickedButtons.Pop(), false);
                }
            );
        }

        void AddListenerForSubmenuButtonClickedRecursively(Button button)
        {
            button?.onClick.AddListener(() =>
                {
                    //button.colors = 
                    // if there's no button input, then push it as a first one.
                    if (ClickedButtons.Count == 0)
                    {
                        ClickedButtons.Push(button.transform);
                    }

                    // check the button is already pushed.
                    if (ClickedButtons.Peek() != button.transform)
                    {
                        ClickedButtons.Push(button.transform);
                    }

                    // open all of the children buttons.
                    ToggleSubMenus(ClickedButtons.Peek(), true);

                    var comp = button.GetComponent<DanbiUIPanelControl>();
                    comp?.OnMenuButtonSelected(ClickedButtons);
                }
            );

            // Get the vertical layout group.
            // 0 -> text(placeholder) so other child is always GetChild(1).
            var submenuVerticalGroup = button.transform.GetChild(1);
            if (submenuVerticalGroup.name.Contains("Vertical"))
            {
                for (int i = 0; i < submenuVerticalGroup.childCount; ++i)
                {
                    var submenuButton = submenuVerticalGroup.GetChild(i).GetComponent<Button>();

                    if (submenuButton.name.Contains("Back"))
                    {
                        AddListenerForBackButtonClicked(submenuButton);
                    }
                    else
                    {
                        AddListenerForSubmenuButtonClickedRecursively(submenuButton);
                    }
                }
            }
            ToggleSubMenus(button.transform, false);
        }

        void ToggleSubMenus(Transform parent, bool flag)
        {
            // child index : 0 -> embedded text, 1 -> vertical layout group.
            parent.GetChild(1).gameObject.SetActive(flag);
        }
    };
};