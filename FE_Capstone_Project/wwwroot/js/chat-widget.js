// chat-widget.js - Chat Widget cho trang Index
(function () {
    'use strict';

    const API_BASE_URL = 'https://localhost:7160/api/chat';
    let connection = null;
    let currentStaffId = null;        // for customer mode
    let currentCustomerId = null;     // for staff mode
    let isWidgetOpen = false;
    let isBotMode = false;             // Track bot mode
    // Helper: Get JWT token
    function getAuthToken() {
        const tokenMeta = document.querySelector('meta[name="jwt-token"]');
        if (tokenMeta) return tokenMeta.getAttribute('content');
        
        const tokenInput = document.getElementById('jwtToken');
        if (tokenInput) return tokenInput.value;
        
        return null;
    }

    // Initialize SignalR connection
    function initializeSignalR() {
        if (typeof signalR === 'undefined') {
            console.error('SignalR library not loaded');
            return;
        }

        const token = getAuthToken();
        if (!token) {
            // For guests, SignalR is not needed (only chatbot is available)
            console.log('No JWT token found - guest mode, chatbot only');
            return;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7160/chathub", {
                accessTokenFactory: () => token,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Warning)
            .build();

        // Listen for new messages
        connection.on("ReceiveMessage", (message) => {
            console.log('Widget received message:', message);
            const currentRoleId = parseInt(document.getElementById('currentRoleId')?.value || '0');
            if (currentRoleId === 3) {
                // Customer chatting with a staff
                if (currentStaffId && message.staffId === currentStaffId) {
                    displayWidgetMessage(message);
                    updateBadge();
                }
            } else if (currentRoleId === 2) {
                // Staff chatting with a customer
                if (currentCustomerId && message.customerId === currentCustomerId) {
                    displayWidgetMessage(message);
                    updateBadge();
                }
            }
        });

        // Listen for notifications
        connection.on("NewMessageNotification", (notification) => {
            console.log('Widget notification:', notification);
            updateBadge();
        });

        // Start connection
        connection.start()
            .then(() => {
                console.log('Chat widget SignalR connected');
            })
            .catch((err) => {
                console.error('Chat widget SignalR connection error:', err);
            });
    }

    // Load staff list (for customer)
    async function loadStaffList() {
        const staffList = document.getElementById('staffList');
        if (!staffList) return;

        try {
            const response = await fetch(`${API_BASE_URL}/staff-list`);
            if (!response.ok) {
                throw new Error('Failed to load staff list');
            }

            const staffs = await response.json();
            staffList.innerHTML = '';

            if (staffs.length === 0) {
                staffList.innerHTML = '<p class="text-center text-muted">No staff available at the moment.</p>';
                return;
            }

            staffs.forEach(staff => {
                const staffItem = document.createElement('div');
                staffItem.className = 'staff-item';
                staffItem.innerHTML = `
                    <div class="staff-name">${staff.fullName || `${staff.firstName} ${staff.lastName}`}</div>
                    <div class="staff-email">${staff.email || ''}</div>
                `;
                staffItem.addEventListener('click', () => selectStaff(staff.id, staff.fullName || `${staff.firstName} ${staff.lastName}`));
                staffList.appendChild(staffItem);
            });
        } catch (error) {
            console.error('Error loading staff list:', error);
            staffList.innerHTML = '<p class="text-center text-danger">Error loading staff list. Please try again.</p>';
        }
    }

    // Load customer conversations (for staff)
    async function loadCustomerConversations() {
        const customerList = document.getElementById('customerList');
        if (!customerList) return;

        try {
            const token = getAuthToken();
            const response = await fetch(`${API_BASE_URL}/conversations`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            if (!response.ok) {
                throw new Error('Failed to load conversations');
            }

            const conversations = await response.json();
            customerList.innerHTML = '';

            if (!conversations || conversations.length === 0) {
                customerList.innerHTML = '<p class="text-center text-muted">No conversations yet.</p>';
                return;
            }

            conversations.forEach(conv => {
                const item = document.createElement('div');
                item.className = 'staff-item';
                const name = conv.customerName || `Customer #${conv.customerId}`;
                const last = conv.lastMessage || 'No messages yet';
                item.innerHTML = `
                    <div class="staff-name">${name}</div>
                    <div class="staff-email">${last}</div>
                `;
                item.addEventListener('click', () => selectCustomer(conv.customerId, name));
                customerList.appendChild(item);
            });
        } catch (error) {
            console.error('Error loading conversations:', error);
            customerList.innerHTML = '<p class="text-center text-danger">Error loading conversations. Please try again.</p>';
        }
    }

    // Select staff and load conversation
    async function selectStaff(staffId, staffName) {
        currentStaffId = staffId;
        
        // Hide staff selection, show chat
        const staffSelection = document.getElementById('staffSelection');
        const messagesContainer = document.getElementById('chatMessagesContainer');
        const inputContainer = document.getElementById('chatInputContainer');
        
        if (staffSelection) staffSelection.style.display = 'none';
        if (messagesContainer) messagesContainer.style.display = 'block';
        if (inputContainer) inputContainer.style.display = 'block';

        // Update header
        const header = document.querySelector('.chat-window-header h6');
        if (header) {
            header.innerHTML = `<i class="bi bi-person-badge"></i> ${staffName}`;
        }

        // Load conversation
        await loadConversation();
        resetWidgetView();
    }

    // Select bot (iframe mode)
    function selectBot() {
        isBotMode = true;
        currentStaffId = null;
        currentCustomerId = null;

        const initialSel = document.getElementById('initialSelection');
        const staffSel = document.getElementById('staffSelection');
        const customerSel = document.getElementById('customerSelection');
        const messagesContainer = document.getElementById('chatMessagesContainer');
        const inputContainer = document.getElementById('chatInputContainer');
        const botContainer = document.getElementById('botContainer');
        const botFrame = document.getElementById('botFrame');

        if (initialSel) initialSel.style.display = 'none';
        if (staffSel) staffSel.style.display = 'none';
        if (customerSel) customerSel.style.display = 'none';
        if (messagesContainer) messagesContainer.style.display = 'none';
        if (inputContainer) inputContainer.style.display = 'none';

        if (botContainer) {
            botContainer.style.display = 'block';
        }

        const header = document.querySelector('.chat-window-header h6');
        if (header) {
            header.innerHTML = `<i class="bi bi-robot"></i> Chat với Bot`;
        }

        if (botFrame && botFrame.contentWindow) {
            botFrame.contentWindow.postMessage({ type: 'BOT_WIDGET_OPEN' }, '*');
        }
    }

    // Select customer and load conversation (for staff)
    async function selectCustomer(customerId, customerName) {
        currentCustomerId = customerId;

        // Hide customer selection, show chat
        const customerSelection = document.getElementById('customerSelection');
        const messagesContainer = document.getElementById('chatMessagesContainer');
        const inputContainer = document.getElementById('chatInputContainer');

        if (customerSelection) customerSelection.style.display = 'none';
        if (messagesContainer) messagesContainer.style.display = 'block';
        if (inputContainer) inputContainer.style.display = 'block';

        // Update header
        const header = document.querySelector('.chat-window-header h6');
        if (header) {
            header.innerHTML = `<i class="bi bi-person-badge"></i> ${customerName}`;
        }

        // Load conversation
        await loadConversation();
        resetWidgetView();
    }

    // Load conversation
    async function loadConversation() {
        const currentUserId = parseInt(document.getElementById('currentUserId')?.value || '0');
        const currentRoleId = parseInt(document.getElementById('currentRoleId')?.value || '0');
        if (currentUserId === 0) {
            console.error('Invalid user ID');
            return;
        }

        const messagesContainer = document.getElementById('chatMessagesContainer');
        if (!messagesContainer) return;

        // Show loading
        messagesContainer.innerHTML = `
            <div class="text-center py-3">
                <div class="spinner-border spinner-border-sm text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="text-muted mt-2 small">Loading messages...</p>
            </div>
        `;

        try {
            const token = getAuthToken();
            if (!token) {
                throw new Error('No authentication token');
            }

            // Determine customerId and staffId based on role
            let customerId, staffId;
            if (currentRoleId === 3) {
                // customer mode
                customerId = currentUserId;
                staffId = currentStaffId;
            } else if (currentRoleId === 2) {
                // staff mode
                customerId = currentCustomerId;
                staffId = currentUserId;
            } else {
                throw new Error('Invalid role for chat');
            }

            const response = await fetch(`${API_BASE_URL}/conversation/${customerId}/${staffId}`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) {
                if (response.status === 401) {
                    throw new Error('Please login again');
                }
                throw new Error('Failed to load conversation');
            }

            const conversation = await response.json();
            messagesContainer.innerHTML = '';

            if (conversation.messages && conversation.messages.length > 0) {
                conversation.messages.forEach(msg => {
                    displayWidgetMessage(msg);
                });
            } else {
                messagesContainer.innerHTML = `
                    <div class="chat-widget-empty">
                        <i class="bi bi-chat-left-text"></i>
                        <p>No messages yet. Start the conversation!</p>
                    </div>
                `;
            }

            scrollWidgetToBottom();
        } catch (error) {
            console.error('Error loading conversation:', error);
            messagesContainer.innerHTML = `
                <div class="text-center py-3">
                    <i class="bi bi-exclamation-triangle text-danger"></i>
                    <p class="text-danger mt-2">${error.message || 'Error loading conversation.'}</p>
                </div>
            `;
        }
    }

    // Display message in widget
    function displayWidgetMessage(message) {
        const messagesContainer = document.getElementById('chatMessagesContainer');
        if (!messagesContainer) return;

        // Remove empty state if exists
        const emptyState = messagesContainer.querySelector('.chat-widget-empty');
        if (emptyState) {
            emptyState.remove();
        }

        const currentUserId = parseInt(document.getElementById('currentUserId')?.value || '0');
        const currentRoleId = parseInt(document.getElementById('currentRoleId')?.value || '0');
        
        // Xác định isSender dựa trên ChatType
        // ChatType: 1 = Customer, 2 = Staff
        let isSender = false;
        if (message.chatType !== undefined && message.chatType !== null) {
            // Nếu ChatType = 1 (Customer) và currentRoleId = 3 (Customer) → tin nhắn của mình
            // Nếu ChatType = 2 (Staff) và currentRoleId = 2 (Staff) → tin nhắn của mình
            if ((message.chatType === 1 && currentRoleId === 3) || 
                (message.chatType === 2 && currentRoleId === 2)) {
                isSender = true;
            }
        } else {
            // Fallback: dùng senderId nếu ChatType không có
            isSender = message.senderId === currentUserId;
        }

        // Check if message already exists
        const existingMessage = messagesContainer.querySelector(`[data-message-id="${message.id}"]`);
        if (existingMessage) {
            return;
        }

        const messageDiv = document.createElement('div');
        messageDiv.className = `chat-widget-message ${isSender ? 'sent' : 'received'}`;
        messageDiv.setAttribute('data-message-id', message.id || Date.now());

        const bubbleDiv = document.createElement('div');
        bubbleDiv.className = `chat-widget-bubble ${isSender ? 'sent' : 'received'}`;

        const messageText = document.createElement('div');
        messageText.className = 'message-text';
        messageText.textContent = message.message || '';

        const timeText = document.createElement('div');
        timeText.className = 'message-time';
        const sentDate = message.sentDate ? new Date(message.sentDate) : new Date();
        timeText.textContent = sentDate.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });

        bubbleDiv.appendChild(messageText);
        bubbleDiv.appendChild(timeText);
        messageDiv.appendChild(bubbleDiv);
        messagesContainer.appendChild(messageDiv);

        scrollWidgetToBottom();
    }

    // Scroll to bottom
    function scrollWidgetToBottom() {
        const messagesContainer = document.getElementById('chatMessagesContainer');
        if (messagesContainer) {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
    }

    // Send message
    async function sendWidgetMessage() {
        const input = document.getElementById('chatWidgetInput');
        const sendButton = document.getElementById('chatWidgetSendButton');
        const currentUserId = parseInt(document.getElementById('currentUserId')?.value || '0');
        const currentRoleId = parseInt(document.getElementById('currentRoleId')?.value || '0');

        // Validate target
        const hasTarget = (currentRoleId === 3 && !!currentStaffId && currentStaffId > 0) || (currentRoleId === 2 && !!currentCustomerId);
        if (!input || !input.value.trim() || !hasTarget || currentUserId === 0) {
            return;
        }

        const message = input.value.trim();
        
        // Disable button
        if (sendButton) {
            sendButton.disabled = true;
            sendButton.innerHTML = '<i class="bi bi-hourglass-split"></i>';
        }

        // Build payload based on role
        // ChatType sẽ được set tự động bởi backend dựa trên role
        let payload;
        if (currentRoleId === 3) {
            // customer - chỉ chat với staff thật
            if (!currentStaffId || currentStaffId <= 0) {
                alert('Please select a staff to chat with.');
                if (sendButton) {
                    sendButton.disabled = false;
                    sendButton.innerHTML = '<i class="bi bi-send"></i>';
                }
                return;
            }
            payload = {
                customerId: currentUserId,
                staffId: currentStaffId,
                message: message,
                senderId: currentUserId
            };
        } else {
            // staff
            if (!currentCustomerId || currentCustomerId === 0) {
                alert('Please select a customer to chat with.');
                if (sendButton) {
                    sendButton.disabled = false;
                    sendButton.innerHTML = '<i class="bi bi-send"></i>';
                }
                return;
            }
            payload = {
                customerId: currentCustomerId,
                staffId: currentUserId,
                message: message,
                senderId: currentUserId
            };
        }

        // Validate payload
        if (!payload.message || payload.message.trim().length === 0) {
            alert('Message cannot be empty.');
            if (sendButton) {
                sendButton.disabled = false;
                sendButton.innerHTML = '<i class="bi bi-send"></i>';
            }
            return;
        }

        try {
            const token = getAuthToken();
            const response = await fetch(`${API_BASE_URL}/send`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                input.value = '';
                // Message will be displayed via SignalR
            } else {
                const errorData = await response.json().catch(() => ({ message: 'Unknown error' }));
                console.error('Error sending message:', errorData);
                
                // Hiển thị lỗi chi tiết hơn
                let errorMessage = errorData.message || 'Unknown error';
                if (errorData.errors && Array.isArray(errorData.errors)) {
                    const errorDetails = errorData.errors.map(e => `${e.field}: ${e.message}`).join('\n');
                    errorMessage += '\n' + errorDetails;
                }
                
                alert('Error sending message: ' + errorMessage);
            }
        } catch (error) {
            console.error('Error sending message:', error);
            alert('Error sending message. Please try again.');
        } finally {
            if (sendButton) {
                sendButton.disabled = false;
                sendButton.innerHTML = '<i class="bi bi-send"></i>';
            }
        }
    }


    // Update badge
    async function updateBadge() {
        const badge = document.getElementById('chatBadge');
        if (!badge) return;

        try {
            const token = getAuthToken();
            if (!token) {
                badge.style.display = 'none';
                return;
            }
            const response = await fetch(`${API_BASE_URL}/unread-count`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                const count = data.unreadCount || 0;
                if (count > 0) {
                    badge.textContent = count > 99 ? '99+' : count;
                    badge.style.display = 'flex';
                } else {
                    badge.style.display = 'none';
                }
            }
        } catch (error) {
            console.error('Error updating badge:', error);
        }
    }

    // Helper to reset sections based on role
    function resetWidgetView() {
        const currentRoleId = parseInt(document.getElementById('currentRoleId')?.value || '0');
        const isLoggedIn = document.getElementById('isLoggedIn')?.value === 'true';
        const initialSel = document.getElementById('initialSelection');
        const staffSel = document.getElementById('staffSelection');
        const customerSel = document.getElementById('customerSelection');
        const msgs = document.getElementById('chatMessagesContainer');
        const input = document.getElementById('chatInputContainer');
        const botContainer = document.getElementById('botContainer');

        if (currentRoleId === 3 || !isLoggedIn) {
            if (initialSel) {
                initialSel.style.display = (!currentStaffId && !isBotMode) ? 'block' : 'none';
            }
            if (staffSel) {
                staffSel.style.display = 'none';
            }
            if (customerSel) customerSel.style.display = 'none';
        } else if (currentRoleId === 2) {
            if (customerSel) customerSel.style.display = currentCustomerId ? 'none' : 'block';
            if (staffSel) staffSel.style.display = 'none';
            if (initialSel) initialSel.style.display = 'none';
        }

        if (botContainer) {
            botContainer.style.display = isBotMode ? 'block' : 'none';
        }

        const shouldShowMessages = !isBotMode && ((currentStaffId && currentRoleId === 3 && isLoggedIn) || (currentCustomerId && currentRoleId === 2));
        if (msgs) msgs.style.display = shouldShowMessages ? 'block' : 'none';
        if (input) input.style.display = shouldShowMessages ? 'block' : 'none';
    }

    // Show initial selection (for customer/guest - show staff option or login prompt)
    function showInitialSelection() {
        const currentRoleId = parseInt(document.getElementById('currentRoleId')?.value || '0');
        const isLoggedIn = document.getElementById('isLoggedIn')?.value === 'true';
        currentStaffId = null;
        isBotMode = false;
        
        const botContainer = document.getElementById('botContainer');
        if (botContainer) {
            botContainer.style.display = 'none';
        }

        if (currentRoleId === 3 || !isLoggedIn) {
            const initialSel = document.getElementById('initialSelection');
            const staffSel = document.getElementById('staffSelection');
            const msgs = document.getElementById('chatMessagesContainer');
            const input = document.getElementById('chatInputContainer');
            
            if (initialSel) initialSel.style.display = 'block';
            if (staffSel) staffSel.style.display = 'none';
            if (msgs) msgs.style.display = 'none';
            if (input) input.style.display = 'none';
        } else if (currentRoleId === 2) {
            const initialSel = document.getElementById('initialSelection');
            const staffSel = document.getElementById('staffSelection');
            const customerSel = document.getElementById('customerSelection');
            const msgs = document.getElementById('chatMessagesContainer');
            const input = document.getElementById('chatInputContainer');
            
            if (initialSel) initialSel.style.display = 'none';
            if (staffSel) staffSel.style.display = 'none';
            if (customerSel) customerSel.style.display = 'block';
            if (msgs) msgs.style.display = 'none';
            if (input) input.style.display = 'none';
            
            currentCustomerId = null;
        }
    }

    // Show staff selection
    function showStaffSelection() {
        const initialSel = document.getElementById('initialSelection');
        const staffSel = document.getElementById('staffSelection');
        
        if (initialSel) initialSel.style.display = 'none';
        if (staffSel) staffSel.style.display = 'block';
        
        loadStaffList();
    }

    // Toggle chat window
    function toggleChatWindow() {
        const chatWindow = document.getElementById('chatWindow');
        const chatButton = document.getElementById('chatWidgetButton');
        
        if (!chatWindow || !chatButton) return;

        isWidgetOpen = !isWidgetOpen;
        
        if (isWidgetOpen) {
            chatWindow.style.display = 'flex';

            const currentRoleId = parseInt(document.getElementById('currentRoleId')?.value || '0');
            const isLoggedIn = document.getElementById('isLoggedIn')?.value === 'true';

            // Reset view to appropriate selection
            resetWidgetView();

            if (currentRoleId === 3 || !isLoggedIn) {
                // Customer or Guest: show initial selection if no staff selected
                if (!currentStaffId) {
                    showInitialSelection();
                }
            } else if (currentRoleId === 2) {
                // Staff: Load customer conversations if not loaded
                const customerList = document.getElementById('customerList');
                if (customerList && (customerList.children.length === 1 || customerList.querySelector('.spinner-border'))) {
                    loadCustomerConversations();
                }
            }
            
            // Initialize SignalR if not connected (only for logged in users)
            if (isLoggedIn && typeof signalR !== 'undefined') {
                if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
                    initializeSignalR();
                }
            }
        } else {
            chatWindow.style.display = 'none';
        }
    }

    // Close chat window
    function closeChatWindow() {
        const chatWindow = document.getElementById('chatWindow');
        if (chatWindow) {
            chatWindow.style.display = 'none';
            isWidgetOpen = false;
        }

        // Reset selection state
        const currentRoleId = parseInt(document.getElementById('currentRoleId')?.value || '0');
        const isLoggedIn = document.getElementById('isLoggedIn')?.value === 'true';
        if (currentRoleId === 3 || !isLoggedIn) {
            currentStaffId = null;
        } else if (currentRoleId === 2) {
            currentCustomerId = null;
        }
        isBotMode = false;
        resetWidgetView();
    }

    // Initialize widget
    function initializeWidget() {
        const chatButton = document.getElementById('chatWidgetButton');
        const closeButton = document.getElementById('chatCloseButton');
        const sendButton = document.getElementById('chatWidgetSendButton');
        const input = document.getElementById('chatWidgetInput');
        const staffOption = document.getElementById('selectStaffOption');
        const botOption = document.getElementById('selectBotOption');

        if (chatButton) {
            chatButton.addEventListener('click', toggleChatWindow);
        }

        if (closeButton) {
            closeButton.addEventListener('click', closeChatWindow);
        }

        if (sendButton) {
            sendButton.addEventListener('click', sendWidgetMessage);
        }

        if (input) {
            input.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    sendWidgetMessage();
                }
            });
        }

        // Staff option click
        if (staffOption) {
            staffOption.addEventListener('click', () => {
                const isLoggedIn = document.getElementById('isLoggedIn')?.value === 'true';
                if (isLoggedIn) {
                    showStaffSelection();
                } else {
                    if (confirm('Bạn cần đăng nhập để chat với nhân viên. Bạn có muốn đăng nhập không?')) {
                        window.location.href = '/Auth/Login';
                    }
                }
            });
        }

        // Bot option click
        if (botOption) {
            botOption.addEventListener('click', () => {
                selectBot();
            });
        }

        // Adjust default view based on role
        resetWidgetView();

        // Load unread count badge (only for logged in users)
        const isLoggedIn = document.getElementById('isLoggedIn')?.value === 'true';
        if (isLoggedIn) {
            updateBadge();
            setInterval(updateBadge, 30000);
        } else {
            const badge = document.getElementById('chatBadge');
            if (badge) {
                badge.style.display = 'none';
            }
        }
    }

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        if (document.getElementById('chatWidget')) {
            initializeWidget();
        }
    });
})();