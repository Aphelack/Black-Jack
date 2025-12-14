# Black Jack Multiplayer

This is a Blazor WebAssembly Hosted application using SignalR for real-time multiplayer Black Jack.

## How to Run on Multiple Devices (LAN)

To play with friends on different devices (Ubuntu, Arch Linux, Windows, Android, iOS) while hosting on an Ubuntu machine, follow these steps.

### 1. Host Setup (Ubuntu)

The "Host" is the computer running the server code.

1.  **Open a Terminal** in the repository root.
2.  **Find your Local IP Address**:
    Run the following command to see your IP address on the local network:
    ```bash
    hostname -I
    ```
    *   It will look something like `192.168.1.15` or `10.0.0.5`.
    *   Write this down.

3.  **Allow the Port through Firewall** (if enabled):
    If you are using `ufw` (Uncomplicated Firewall), allow traffic on port `5220`:
    ```bash
    sudo ufw allow 5220/tcp
    ```

4.  **Run the Server**:
    Navigate to the solution folder and run the server, binding it to all network interfaces (`0.0.0.0`) so other devices can see it.
    ```bash
    cd BlackJack
    dotnet run --project BlackJack.Server/BlackJack.Server.csproj --urls "http://0.0.0.0:5220"
    ```

### 2. Client Setup (Any Device)

The "Client" is any device that wants to join the game. This can be the Host machine itself, or another computer/phone on the same Wi-Fi.

1.  **Connect to the same Network**: Ensure the device is connected to the same Wi-Fi or LAN as the Host.
2.  **Open a Web Browser**: Use a modern browser like Chrome, Firefox, Edge, or Safari.
3.  **Navigate to the Game**:
    Enter the Host's IP address followed by the port `:5220`.
    
    **Format:**
    ```
    http://<HOST_IP_ADDRESS>:5220
    ```

    **Example:**
    If the Host's IP is `192.168.1.15`, enter:
    ```
    http://192.168.1.15:5220
    ```

### 3. Playing the Game

1.  **Player 1 (Host)**: Open the browser on the host machine (you can use `http://localhost:5220` or the IP).
    *   Enter your name.
    *   Enter a **Room Name** (e.g., "Casino").
    *   Click **Create Game**.
    *   Share the **Game ID** (displayed in the waiting lobby) with your friends.

2.  **Player 2+ (Clients)**:
    *   Enter your name.
    *   Enter the **Game ID** provided by Player 1.
    *   Click **Join Game**.

3.  **Start**: Once everyone has joined, the Host clicks **Start Game**.

### Troubleshooting

*   **"This site can't be reached"**: 
    *   Double-check the IP address of the Host.
    *   Ensure the Host server is running.
    *   Check if the Host's firewall is blocking port 5220.
    *   Ensure devices are on the same network (e.g., not one on Guest Wi-Fi and one on Main).
*   **Game disconnects**: SignalR requires a stable connection. If on mobile, ensure the screen stays on or the browser doesn't put the tab to sleep.
