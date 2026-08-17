// Global Keyboard Shortcut Handler
window.KeyboardShortcutHandler = {
    dotnetInstance: null,
    
    initialize: function(dotnetRef) {
        this.dotnetInstance = dotnetRef;
        document.addEventListener('keydown', (e) => this.handleKeyDown(e));
    },
    
    handleKeyDown: function(e) {
        // Ignore if user is typing in an input/textarea
        const target = e.target;
        const isInput = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA';
        
        // Allow normal input for text fields (except when using ctrl/alt/cmd)
        if (isInput && !e.ctrlKey && !e.altKey && !e.metaKey) {
            return;
        }
        
        // List of shortcuts to prevent browser defaults
        const shortcuts = [
            'ctrl+n', 'ctrl+alt+n', // New
            'ctrl+s', // Suppliers (conflicts with Save)
            'ctrl+shift+s', // New Supplier
            'ctrl+p', // Products
            'ctrl+c', // Customers
            'ctrl+shift+c', // New Customer
            'ctrl+r', // Reports
            'ctrl+,', // Settings
            'ctrl+/', // Help/Guide
            'ctrl+f', // Search (on specific pages)
            'ctrl+h', // Home
            'ctrl+e', // Edit
            'ctrl+d', // Delete/Dues
            'ctrl+alt+s', // Save (alt version)
            'ctrl+shift+l', // Logout
        ];
        
        const keyCombo = this.getKeyCombo(e);
        
        if (shortcuts.includes(keyCombo)) {
            e.preventDefault();
        }
        
        // Send to Blazor if it's a registered shortcut
        if (this.dotnetInstance) {
            const result = this.dotnetInstance.invokeMethodAsync('HandleKeyboardShortcut', 
                e.key, 
                e.ctrlKey, 
                e.shiftKey, 
                e.altKey
            );
        }
    },
    
    getKeyCombo: function(e) {
        const keys = [];
        if (e.ctrlKey) keys.push('ctrl');
        if (e.altKey) keys.push('alt');
        if (e.shiftKey) keys.push('shift');
        keys.push(e.key.toLowerCase());
        return keys.join('+');
    }
};

// Also handle in Blazor side for better integration
window.addEventListener('keydown', function(e) {
    // Handle Escape to close modals
    if (e.key === 'Escape') {
        const modal = document.querySelector('.shortcut-guide-overlay');
        if (modal && !modal.hidden) {
            e.preventDefault();
        }
    }
});
