using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnsoulSharp.SDK.MenuUI.Values;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace Stigma.Maokai
{
    public class MeuUI
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="display_name"></param>
        /// <returns></returns>
        public static Menu CreateMainMenu(string name, string display_name)
        {
            Menu main_menu;
            main_menu = new Menu(name, display_name, false);
            return main_menu;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="name"></param>
        /// <param name="display_name"></param>
        /// <param name="value"></param>
        public static void AddMenuBool(Menu menu, string name, string display_name, bool value)
        {
            menu.Add(new MenuBool(name, display_name, value));
        }
        public static void AddMenuBool(Menu menu, string name, string display_name)
        {
            menu.Add(new MenuBool(name, display_name));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="name"></param>
        /// <param name="display_name"></param>
        /// <param name="value"></param>
        /// <param name="min_value"></param>
        /// <param name="max_value"></param>
        public static void AddMenuSlider(Menu menu, string name, string display_name, int value, int min_value,
            int max_value)
        {
            menu.Add(new MenuSlider(name, display_name, value, min_value, max_value));
        }
        public static void AddMenuSlider(Menu menu, string name, string display_name, int value)
        {
            menu.Add(new MenuSlider(name, display_name, value));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="name"></param>
        /// <param name="display_name"></param>
        /// <param name="key"></param>
        /// <param name="type"></param>
        public static void AddMenuKeyBind(Menu menu, string name, string display_name, Keys key, KeyBindType type)
        {
            menu.Add(new MenuKeyBind(name, display_name, key, type));
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="main_menu"></param>
        /// <param name="sub_menu_name"></param>
        /// <param name="sub_menu_item_name"></param>
        /// <returns></returns>
        public static bool GetMenuBoolValue(Menu main_menu, string sub_menu_name, string sub_menu_item_name)
        {
            return main_menu[sub_menu_name][sub_menu_item_name].GetValue<MenuBool>().Enabled;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="main_menu"></param>
        /// <param name="sub_menu_name"></param>
        /// <param name="sub_menu_item_name"></param>
        /// <returns></returns>
        public static int GetMenuSliderValue(Menu main_menu, string sub_menu_name, string sub_menu_item_name)
        {
            return main_menu[sub_menu_name][sub_menu_item_name].GetValue<MenuSlider>().Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="main_menu"></param>
        /// <param name="sub_menu_name"></param>
        /// <param name="sub_menu_item_name"></param>
        /// <returns></returns>
        public static bool GetMenuKeyBindValue(Menu main_menu, string sub_menu_name, string sub_menu_item_name)
        {
            return main_menu[sub_menu_name][sub_menu_item_name].GetValue<MenuKeyBind>().Active;
        }

        public static void AddMenuList(Menu mainMenu, string sub_menu_name, string sub_menu_item_name, string[] list)
        {
            mainMenu.Add(new MenuList(sub_menu_name, sub_menu_item_name, list));
        }

        public static string GetMenuList(Menu mainMenu, string sub_menu_name, string sub_menu_item_name)
        {
            return mainMenu[sub_menu_name][sub_menu_item_name].GetValue<MenuList>().SelectedValue;
        }
    }
}
