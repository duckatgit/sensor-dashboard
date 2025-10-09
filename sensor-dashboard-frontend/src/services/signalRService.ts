import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr";

let connection: HubConnection;

export const startConnection = async () => {
  connection = new HubConnectionBuilder()
    .withUrl("https://localhost:7020/sensorHub") // Backend URL
    .withAutomaticReconnect()
    .build();

  try {
    await connection.start();
    console.log("SignalR Connected");
  } catch (err) {
    console.error("Error connecting to SignalR:", err);
  }

  return connection;
};

export const getConnection = () => connection;
