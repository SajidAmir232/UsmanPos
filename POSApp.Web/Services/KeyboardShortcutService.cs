using System;
using System.Collections.Generic;
using System.Linq;

namespace POSApp.Web.Services
{
    public class KeyboardShortcutService
    {
        private Dictionary<string, ShortcutHandler> _shortcuts = new();
        private string _currentContext = "global";

        public event Action<ShortcutEvent>? OnShortcut;

        public KeyboardShortcutService()
        {
            RegisterGlobalShortcuts();
        }

        private void RegisterGlobalShortcuts()
        {
            // Global Navigation Shortcuts
            Register("global", "ctrl+h", "Home/Dashboard", () => NavigateTo(""), ShortcutCategory.Navigation);
            Register("global", "ctrl+p", "Products", () => NavigateTo("products"), ShortcutCategory.Navigation);
            Register("global", "ctrl+c", "Customers", () => NavigateTo("customers"), ShortcutCategory.Navigation);
            Register("global", "ctrl+shift+c", "New Customer", () => HandleContextAction("create-customer"), ShortcutCategory.Navigation);
            Register("global", "ctrl+s", "Suppliers", () => NavigateTo("suppliers"), ShortcutCategory.Navigation);
            Register("global", "ctrl+shift+s", "New Supplier", () => HandleContextAction("create-supplier"), ShortcutCategory.Navigation);
            Register("global", "ctrl+r", "Reports", () => NavigateTo("reports"), ShortcutCategory.Navigation);
            Register("global", "ctrl+,", "Settings", () => NavigateTo("settings"), ShortcutCategory.Navigation);
            Register("global", "ctrl+/", "Show Keyboard Guide", () => OnShortcut?.Invoke(new ShortcutEvent { Action = "show-guide" }), ShortcutCategory.Help);
            Register("global", "?", "Help", () => OnShortcut?.Invoke(new ShortcutEvent { Action = "show-guide" }), ShortcutCategory.Help);

            // POS/Sales Page Shortcuts
            Register("pos", "ctrl+n", "New Sale", () => HandleContextAction("new-sale"), ShortcutCategory.Action);
            Register("pos", "ctrl+alt+n", "New Sale (Alternative)", () => HandleContextAction("new-sale"), ShortcutCategory.Action);
            Register("pos", "enter", "Add Item to Cart", () => HandleContextAction("add-item"), ShortcutCategory.Action);
            Register("pos", "delete", "Remove Item from Cart", () => HandleContextAction("remove-item"), ShortcutCategory.Action);
            Register("pos", "tab", "Move to Payment", () => HandleContextAction("focus-payment"), ShortcutCategory.Navigation);
            Register("pos", "ctrl+f", "Product Search", () => HandleContextAction("focus-search"), ShortcutCategory.Focus);
            Register("pos", "escape", "Clear Cart", () => HandleContextAction("clear-cart"), ShortcutCategory.Action);

            // Products Page Shortcuts
            Register("products", "ctrl+n", "Add New Product", () => HandleContextAction("add-product"), ShortcutCategory.Action);
            Register("products", "ctrl+f", "Search Products", () => HandleContextAction("focus-search"), ShortcutCategory.Focus);
            Register("products", "ctrl+e", "Edit Selected Product", () => HandleContextAction("edit-product"), ShortcutCategory.Action);
            Register("products", "delete", "Delete Selected Product", () => HandleContextAction("delete-product"), ShortcutCategory.Action);

            // Customers Page Shortcuts
            Register("customers", "ctrl+n", "Add New Customer", () => HandleContextAction("add-customer"), ShortcutCategory.Action);
            Register("customers", "ctrl+f", "Search Customers", () => HandleContextAction("focus-search"), ShortcutCategory.Focus);
            Register("customers", "ctrl+e", "Edit Selected Customer", () => HandleContextAction("edit-customer"), ShortcutCategory.Action);
            Register("customers", "ctrl+d", "View Dues", () => HandleContextAction("view-dues"), ShortcutCategory.Action);

            // Suppliers Page Shortcuts
            Register("suppliers", "ctrl+n", "Add New Supplier", () => HandleContextAction("add-supplier"), ShortcutCategory.Action);
            Register("suppliers", "ctrl+f", "Search Suppliers", () => HandleContextAction("focus-search"), ShortcutCategory.Focus);
            Register("suppliers", "ctrl+e", "Edit Selected Supplier", () => HandleContextAction("edit-supplier"), ShortcutCategory.Action);

            // Purchases Page Shortcuts
            Register("purchases", "ctrl+n", "New Purchase", () => HandleContextAction("new-purchase"), ShortcutCategory.Action);
            Register("purchases", "ctrl+f", "Search Purchases", () => HandleContextAction("focus-search"), ShortcutCategory.Focus);
            Register("purchases", "ctrl+e", "Edit Purchase", () => HandleContextAction("edit-purchase"), ShortcutCategory.Action);

            // Sales Page Shortcuts
            Register("sales", "ctrl+n", "New Sale", () => HandleContextAction("new-sale"), ShortcutCategory.Action);
            Register("sales", "ctrl+f", "Search Sales", () => HandleContextAction("focus-search"), ShortcutCategory.Focus);
            Register("sales", "ctrl+e", "Edit Sale", () => HandleContextAction("edit-sale"), ShortcutCategory.Action);

            // Common Shortcuts across all pages
            Register("global", "ctrl+alt+s", "Save", () => HandleContextAction("save"), ShortcutCategory.Action);
            Register("global", "escape", "Close Modal/Dialog", () => OnShortcut?.Invoke(new ShortcutEvent { Action = "close-modal" }), ShortcutCategory.Navigation);
            Register("global", "ctrl+shift+l", "Logout", () => HandleContextAction("logout"), ShortcutCategory.Action);
        }

        public void Register(string context, string keyCombination, string description, Action handler, ShortcutCategory category)
        {
            var key = $"{context}:{keyCombination.ToLower()}";
            _shortcuts[key] = new ShortcutHandler
            {
                Context = context,
                KeyCombination = keyCombination,
                Description = description,
                Handler = handler,
                Category = category
            };
        }

        public void SetContext(string context)
        {
            _currentContext = context;
        }

        public bool HandleKeyDown(string key, bool ctrlKey, bool shiftKey, bool altKey)
        {
            var keyCombination = BuildKeyCombination(key, ctrlKey, shiftKey, altKey);

            // Try current context first
            if (TryExecuteShortcut(_currentContext, keyCombination))
                return true;

            // Then try global context
            if (_currentContext != "global" && TryExecuteShortcut("global", keyCombination))
                return true;

            return false;
        }

        private bool TryExecuteShortcut(string context, string keyCombination)
        {
            var key = $"{context}:{keyCombination.ToLower()}";
            if (_shortcuts.TryGetValue(key, out var handler))
            {
                handler.Handler?.Invoke();
                OnShortcut?.Invoke(new ShortcutEvent 
                { 
                    Action = handler.Description,
                    Context = context,
                    KeyCombination = keyCombination
                });
                return true;
            }
            return false;
        }

        private string BuildKeyCombination(string key, bool ctrlKey, bool shiftKey, bool altKey)
        {
            var combo = new List<string>();
            if (ctrlKey) combo.Add("ctrl");
            if (altKey) combo.Add("alt");
            if (shiftKey) combo.Add("shift");
            combo.Add(key.ToLower());
            return string.Join("+", combo);
        }

        private void NavigateTo(string path)
        {
            OnShortcut?.Invoke(new ShortcutEvent { Action = "navigate", Value = path });
        }

        private void HandleContextAction(string action)
        {
            OnShortcut?.Invoke(new ShortcutEvent { Action = action, Context = _currentContext });
        }

        public List<ShortcutHandler> GetShortcutsForContext(string context)
        {
            return _shortcuts.Values
                .Where(s => s.Context == context || s.Context == "global")
                .GroupBy(s => s.Category)
                .SelectMany(g => g)
                .ToList();
        }

        public List<string> GetAvailableContexts()
        {
            return _shortcuts.Keys
                .Select(k => k.Split(':')[0])
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
    }

    public class ShortcutHandler
    {
        public string Context { get; set; } = "";
        public string KeyCombination { get; set; } = "";
        public string Description { get; set; } = "";
        public Action? Handler { get; set; }
        public ShortcutCategory Category { get; set; }
    }

    public class ShortcutEvent
    {
        public string Action { get; set; } = "";
        public string Context { get; set; } = "global";
        public string KeyCombination { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public enum ShortcutCategory
    {
        Navigation,
        Action,
        Focus,
        Help
    }
}
