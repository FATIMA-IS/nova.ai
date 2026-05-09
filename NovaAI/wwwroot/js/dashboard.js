function handleKey(e) {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
}

async function sendMessage() {
    const input = document.getElementById('chatInput');
    const msgText = input.value.trim();
    if (!msgText) return;

    // Kullanıcı mesajını ekrana ekle
    appendMessage(msgText, 'user');
    input.value = '';

    // Yükleniyor durumunu Agent loglarına ekle
    addAgentLog('LLM', 'Gemini analiz ediyor...', 'wait');

    try {
        const response = await fetch('/Analysis/Chat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Message: msgText,
                ChannelId: typeof CHANNEL_ID !== 'undefined' ? CHANNEL_ID : ''
            })
        });

        const data = await response.json();

        if (data.success) {
            appendMessage(data.reply, 'bot');
            addAgentLog('RESULT', 'Analiz Tamamlandı', 'success');
        } else {
            appendMessage("Üzgünüm, bir hata oluştu: " + data.error, 'bot');
            addAgentLog('ERROR', 'API Hatası', 'fail');
        }
    } catch (err) {
        appendMessage("Sunucuya bağlanılamadı.", 'bot');
    }
}

function appendMessage(text, sender) {
    const chatContainer = document.getElementById('chatMessages');
    const isBot = sender === 'bot';

    // Markdown/HTML formatlı yanıtları işlemek için (basit replace)
    const formattedText = text.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
        .replace(/\n/g, '<br/>');

    const msgHtml = `
        <div class="msg ${sender}">
            <div class="msg-avatar ${isBot ? 'bot' : ''}">${isBot ? 'N' : 'Y'}</div>
            <div class="msg-body">
                <div class="msg-meta" style="font-size:0.8rem; color:#94a3b8; margin-bottom:4px;">
                    ${isBot ? 'Nova AI' : 'Kullanıcı'}
                </div>
                <div class="msg-bubble ${isBot ? 'bot' : ''}">
                    ${formattedText}
                </div>
            </div>
        </div>
    `;

    chatContainer.insertAdjacentHTML('beforeend', msgHtml);
    chatContainer.scrollTop = chatContainer.scrollHeight;

    // Mesaj sayacını güncelle
    const countEl = document.getElementById('msgCount');
    if (countEl) countEl.innerText = parseInt(countEl.innerText) + 1;
}

function addAgentLog(type, message, status) {
    const logsContainer = document.getElementById('agentEvents');
    let colorClass = type === 'LLM' ? 'var(--accent3)' : (type === 'ERROR' ? '#ef4444' : 'var(--accent)');

    const logHtml = `
        <div class="event" style="border-left-color: ${colorClass}; animation: fadeIn 0.3s ease;">
            <div class="event-header">
                <span class="event-type" style="color:${colorClass}">${type}</span>
                <span class="event-name">${message}</span>
            </div>
        </div>
    `;
    logsContainer.insertAdjacentHTML('afterbegin', logHtml);
}