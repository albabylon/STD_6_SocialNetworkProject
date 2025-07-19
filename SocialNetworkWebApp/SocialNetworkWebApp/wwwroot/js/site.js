class Chat {
    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/NewMessage")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.initEvents();
        this.startConnection();
    }

    initEvents() {
        // Обработчик получения сообщений
        this.connection.on("ReceiveMessage", (user, message) => {
            this.addMessageToChat(user, message);
        });

        // Обработчик кнопки отправки
        document.getElementById("sendButton")?.addEventListener("click", (e) => {
            e.preventDefault();
            this.sendMessage();
        });
    }

    async startConnection() {
        try {
            await this.connection.start();
            console.log("SignalR Connected.");
        } catch (err) {
            console.log(err);
            setTimeout(() => this.startConnection(), 5000);
        }
    }

    async sendMessage() {
        const user = document.getElementById("userInput").value;
        const message = document.getElementById("messageInput").value;

        try {
            await this.connection.invoke("SendMessage", user, message);
            document.getElementById("messageInput").value = "";
        } catch (err) {
            console.error(err);
        }
    }

    addMessageToChat(user, message) {
        const msg = `${user}: ${message}`;
        const div = document.createElement("div");
        div.textContent = msg;
        document.getElementById("messagesList").appendChild(div);
    }
}

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', () => {
    window.chat = new Chat();
});
