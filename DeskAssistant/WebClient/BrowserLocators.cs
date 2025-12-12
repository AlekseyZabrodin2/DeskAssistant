namespace DeskAssistant.WebClient
{
    public class BrowserLocators
    {
        // Переходы
        public readonly string CafeBalukUrl = "https://cafebaluk.by/";
        public readonly string WeekMenuUrl = "https://cafebaluk.by/menu/week";

        // Авторизация
        public readonly string AuthorisationButton = "[title='Авторизация']";
        public readonly string PhoneSelectorField = "#client_phone";
        public readonly string PasswordSelectorField = "#client_pass";
        public readonly string LoginButton = "#cart-login-btn";

        // Навигация
        public readonly string MenuButton = "text=МЕНЮ";
        public readonly string WeekMenuButton = "text=НА НЕДЕЛЮ";
        public readonly string DropdownMenu = "ul.rd-navbar-dropdown.menu-img-wrap.rd-navbar-open-right";

        // Заказы
        public readonly string DateHeader = "div.h4.text-left.font-default.offset-top-60";
        public readonly string OrderTable = "table.table-shopping-cart";
        public readonly string ProductLink = "a.link-default.product-link";
        public readonly string ElementSibling = "el => el.nextElementSibling";
        public readonly string OrderHistory = "text=История заказов";

        // Кнопки заказа
        public readonly string SubmitOrderButton = ".cart-send-a";
    }
}
