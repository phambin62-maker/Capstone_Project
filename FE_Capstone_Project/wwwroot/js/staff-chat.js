// staff-chat.js - Staff Chat Page Functionality
(function () {
    'use strict';

    const API_BASE_URL = 'https://localhost:7160/api/chat';
    let connection = null;
    let currentCustomerId = null;
    let currentStaffId = null;

    // Helper: Get JWT token
    function getAuthToken() {
        const tokenInput = document.getElementById('jwtToken');
        if (tokenInput) return tokenInput.value;
        return null;
    }

    // Helper: Get current user ID
    function getCurrentUserId() {
        const userIdInput = document.getElementById('currentUserId');
        if (userIdInput) return parseInt(userIdInput.value) || 0;
        return 0;
    }

    // Initialize SignalR connection
    function initializeSignalR() {
        if (typeof signalR === 'undefined') {
            console.error('SignalR library not loaded');
            return;
        }

        const token = getAuthToken();
        if (!token) {
            console.error('No JWT token found');
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
            console.log('Staff chat received message:', message);
            if (currentCustomerId && message.customerId === currentCustomerId) {
                // Ensure chatType is set correctly based on senderId
                if (!message.chatType && message.senderId) {
                    message.chatType = (message.senderId === currentStaffId) ? 2 : 1;
                }
                displayMessage(message);
                updateConversationList();
            }
        });

        // Listen for notifications
        connection.on("NewMessageNotification", (notification) => {
            console.log('Staff chat notification:', notification);
            updateConversationList();
        });

        // Start connection
        connection.start()
            .then(() => {
                console.log('Staff chat SignalR connected');
            })
            .catch((err) => {
                console.error('Staff chat SignalR connection error:', err);
            });
    }

    // Load all customers and merge with conversations
    async function loadCustomerConversations() {
        const conversationList = document.getElementById('conversationList');
        if (!conversationList) return;

        try {
            const token = getAuthToken();
            
            // Load both customers and conversations in parallel
            const [customersResponse, conversationsResponse] = await Promise.all([
                fetch(`${API_BASE_URL}/customer-list`, {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                }),
                fetch(`${API_BASE_URL}/conversations`, {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                })
            ]);

            if (!customersResponse.ok) {
                throw new Error('Failed to load customers');
            }

            const customers = await customersResponse.json();
            const conversations = conversationsResponse.ok ? await conversationsResponse.json() : [];

            conversationList.innerHTML = '';

            if (!customers || customers.length === 0) {
                conversationList.innerHTML = '<div class="text-center p-3"><p class="text-muted">No customers found.</p></div>';
                return;
            }

            // Create a map of conversations by customerId for quick lookup
            const conversationMap = new Map();
            if (conversations && conversations.length > 0) {
                conversations.forEach(conv => {
                    conversationMap.set(conv.customerId, conv);
                });
            }

            // Display all customers, with conversation info if available
            customers.forEach(customer => {
                const conv = conversationMap.get(customer.id);
                const item = document.createElement('div');
                const hasConversation = !!conv;
                item.className = `conversation-item ${hasConversation ? 'has-conversation' : 'no-conversation'}`;
                item.setAttribute('data-customer-id', customer.id);
                
                const name = customer.fullName || customer.firstName || customer.lastName || `Customer #${customer.id}`;
                const lastMessage = conv?.lastMessage || 'No messages yet';
                let lastMessageDate = '';
                if (conv?.lastMessageDate) {
                    const dateStr = conv.lastMessageDate;
                    // Check if date string has timezone info
                    const hasTimezone = dateStr.includes('Z') || dateStr.includes('+') || dateStr.match(/-\d{2}:\d{2}$/);
                    
                    let date;
                    if (hasTimezone) {
                        date = new Date(dateStr);
                    } else {
                        // Assume UTC if no timezone info
                        date = new Date(dateStr + (dateStr.endsWith('Z') ? '' : 'Z'));
                    }
                    
                    // Validate date
                    if (!isNaN(date.getTime())) {
                        lastMessageDate = date.toLocaleString('vi-VN', {
                            month: 'short',
                            day: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit',
                            hour12: false
                        });
                    }
                }
                
                const unreadBadge = conv && conv.unreadCount > 0 
                    ? `<span class="badge bg-danger ms-2">${conv.unreadCount}</span>` 
                    : '';

                item.innerHTML = `
                    <div class="conversation-name">${name}${unreadBadge}</div>
                    <div class="conversation-preview">${lastMessage}</div>
                    ${lastMessageDate ? `<small class="text-muted">${lastMessageDate}</small>` : ''}
                `;
                
                item.addEventListener('click', () => selectConversation(customer.id, name));
                conversationList.appendChild(item);
            });
        } catch (error) {
            console.error('Error loading customers:', error);
            conversationList.innerHTML = '<div class="text-center p-3"><p class="text-danger">Error loading customers. Please try again.</p></div>';
        }
    }

    // Select conversation and load messages
    async function selectConversation(customerId, customerName) {
        currentCustomerId = customerId;
        currentStaffId = getCurrentUserId();

        // Update active state in conversation list
        const conversationItems = document.querySelectorAll('.conversation-item');
        conversationItems.forEach(item => {
            item.classList.remove('active');
            if (parseInt(item.getAttribute('data-customer-id')) === customerId) {
                item.classList.add('active');
            }
        });

        // Show chat content, hide empty state
        const emptyState = document.getElementById('emptyState');
        const chatMainContent = document.getElementById('chatMainContent');
        
        if (emptyState) emptyState.style.display = 'none';
        if (chatMainContent) chatMainContent.style.display = 'flex';

        // Update header
        const chatHeaderTitle = document.getElementById('chatHeaderTitle');
        if (chatHeaderTitle) {
            chatHeaderTitle.textContent = customerName;
        }

        // Load conversation messages
        await loadConversation();
    }

    // Load conversation messages
    async function loadConversation() {
        const chatMessages = document.getElementById('chatMessages');
        if (!chatMessages || !currentCustomerId || !currentStaffId) return;

        // Show loading
        chatMessages.innerHTML = `
            <div class="text-center py-3">
                <div class="spinner-border spinner-border-sm text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="text-muted mt-2 small">Loading messages...</p>
            </div>
        `;

        try {
            const token = getAuthToken();
            const response = await fetch(`${API_BASE_URL}/conversation/${currentCustomerId}/${currentStaffId}`, {
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
            chatMessages.innerHTML = '';

            if (conversation.messages && conversation.messages.length > 0) {
                conversation.messages.forEach(msg => {
                    displayMessage(msg);
                });
            } else {
                chatMessages.innerHTML = `
                    <div class="empty-state">
                        <i class="bi bi-chat-left-text"></i>
                        <p>No messages yet. Start the conversation!</p>
                    </div>
                `;
            }

            scrollToBottom();
        } catch (error) {
            console.error('Error loading conversation:', error);
            chatMessages.innerHTML = `
                <div class="text-center py-3">
                    <i class="bi bi-exclamation-triangle text-danger"></i>
                    <p class="text-danger mt-2">${error.message || 'Error loading conversation.'}</p>
                </div>
            `;
        }
    }

    // Display message
    function displayMessage(message) {
        const chatMessages = document.getElementById('chatMessages');
        if (!chatMessages) return;

        // Remove empty state if exists
        const emptyState = chatMessages.querySelector('.empty-state');
        if (emptyState) {
            emptyState.remove();
        }

        const currentUserId = getCurrentUserId();
        
        // Determine if message is sent by current user (staff)
        // ChatType: 1 = Customer, 2 = Staff
        const isSender = message.chatType === 2;

        // Check if message already exists
        const existingMessage = chatMessages.querySelector(`[data-message-id="${message.id}"]`);
        if (existingMessage) {
            return;
        }

        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${isSender ? 'sent' : 'received'}`;
        messageDiv.setAttribute('data-message-id', message.id || Date.now());

        const bubbleDiv = document.createElement('div');
        bubbleDiv.className = 'message-bubble';

        const messageText = document.createElement('div');
        messageText.textContent = message.message || '';

        const timeText = document.createElement('div');
        timeText.className = 'message-time';
        let sentDate;
        if (message.sentDate) {
            // Parse the date string
            const dateStr = message.sentDate;
            // Check if date string has timezone info
            const hasTimezone = dateStr.includes('Z') || dateStr.includes('+') || dateStr.match(/-\d{2}:\d{2}$/);
            
            if (hasTimezone) {
                // Date string already has timezone info, parse directly
                sentDate = new Date(dateStr);
            } else {
                // Date string doesn't have timezone, assume it's UTC and add 'Z'
                sentDate = new Date(dateStr + (dateStr.endsWith('Z') ? '' : 'Z'));
            }
            
            // Validate the date
            if (isNaN(sentDate.getTime())) {
                // If parsing failed, use current time
                sentDate = new Date();
            }
        } else {
            sentDate = new Date();
        }
        
        // Format time in local timezone (Vietnam time - UTC+7)
        // JavaScript automatically converts UTC to local time, but we can specify timezone explicitly
        timeText.textContent = sentDate.toLocaleTimeString('vi-VN', { 
            hour: '2-digit', 
            minute: '2-digit',
            hour12: false
        });

        bubbleDiv.appendChild(messageText);
        bubbleDiv.appendChild(timeText);
        messageDiv.appendChild(bubbleDiv);
        chatMessages.appendChild(messageDiv);

        scrollToBottom();
    }

    // Send message
    async function sendMessage() {
        const messageInput = document.getElementById('messageInput');
        const sendButton = document.getElementById('sendMessageBtn');

        if (!messageInput || !messageInput.value.trim() || !currentCustomerId || !currentStaffId) {
            return;
        }

        const message = messageInput.value.trim();

        // Disable button
        if (sendButton) {
            sendButton.disabled = true;
            sendButton.innerHTML = '<i class="bi bi-hourglass-split"></i>';
        }

        try {
            const token = getAuthToken();
            const payload = {
                customerId: currentCustomerId,
                staffId: currentStaffId,
                message: message,
                senderId: currentStaffId
            };

            const response = await fetch(`${API_BASE_URL}/send`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                const responseData = await response.json().catch(() => null);
                messageInput.value = '';
                
                // Display message immediately (optimistic update)
                if (responseData) {
                    // Ensure chatType is set to Staff (2) and add senderId
                    responseData.chatType = 2;
                    responseData.senderId = currentStaffId;
                    responseData.customerId = currentCustomerId;
                    responseData.staffId = currentStaffId;
                    // If sentDate is missing, use current time
                    if (!responseData.sentDate) {
                        responseData.sentDate = new Date().toISOString();
                    }
                    displayMessage(responseData);
                }
                
                // Refresh conversation list to update last message and unread count
                setTimeout(() => {
                    updateConversationList();
                }, 500);
            } else {
                const errorData = await response.json().catch(() => ({ message: 'Unknown error' }));
                console.error('Error sending message:', errorData);
                alert('Error sending message: ' + (errorData.message || 'Unknown error'));
            }
        } catch (error) {
            console.error('Error sending message:', error);
            alert('Error sending message. Please try again.');
        } finally {
            if (sendButton) {
                sendButton.disabled = false;
                sendButton.innerHTML = '<i class="bi bi-send-fill"></i>';
            }
        }
    }

    // Scroll to bottom
    function scrollToBottom() {
        const chatMessages = document.getElementById('chatMessages');
        if (chatMessages) {
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }
    }

    // Update conversation list (refresh after sending/receiving messages)
    async function updateConversationList() {
        await loadCustomerConversations();
        
        // Re-select current conversation if exists
        if (currentCustomerId) {
            const conversationItems = document.querySelectorAll('.conversation-item');
            conversationItems.forEach(item => {
                if (parseInt(item.getAttribute('data-customer-id')) === currentCustomerId) {
                    item.classList.add('active');
                }
            });
        }
    }

    // Initialize
    function initialize() {
        const sendButton = document.getElementById('sendMessageBtn');
        const messageInput = document.getElementById('messageInput');

        if (sendButton) {
            sendButton.addEventListener('click', sendMessage);
        }

        if (messageInput) {
            messageInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    sendMessage();
                }
            });
        }

        // Load conversations on page load
        loadCustomerConversations();

        // Initialize SignalR
        initializeSignalR();
    }

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        initialize();
    });
})();

