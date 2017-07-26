using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Host : MonoBehaviour {
	public GameObject FlareTemplate;
	int RegisteredPool;
	void Awake(){
		RegisteredPool = Pool.Register (FlareTemplate);
	}

	int ConnectedPlayers;
	struct Connection{
		public int UniquePlayerID;
		public string name;

		// TODO(lubomir): These probably shouldn't be here lol
		public Vector3 pos;
		public Transform flare;
	}

	enum State{
		None,
		Client,
		Server
	}

	public Color networkColor;
	public Color serverMessageColor;

	State state;

	// stuff
	int hostId;
	int recHostId;
	int connectionId;
	int channelId;
	byte error;

	int MyConnectionID;

	const int MESSAGE_SIZE = 1;
	public enum Message : byte{
		PlayerConnected,
		PlayerDisconnected,
		PlayerRename,
		PlayerOnConnect,
		ServerMessage,
		Chat
	}

	Buffer buffer;
	struct Buffer{
		/*public Buffer(int size){
			bufferSize = size;
			buffer = new byte[size];
		}*/

		public byte[] buffer;
		public int bufferSize;
		public int data;
		public int at;

		public void WriteHeader(Message message){
			buffer [0] = (byte)message;
			at = MESSAGE_SIZE;
		}

		// string
		public void Write(string s){
			byte length = (byte)s.Length;
			buffer [at] = length;
			at += 1;

			int size = length * sizeof(char);
			System.Buffer.BlockCopy (s.ToCharArray (), 0, buffer, at, size);
			at += size;
		}

		public void Read(out string s){
			byte length = buffer [at];
			at += 1;

			char[] chars = new char[length];
			int size = length * sizeof(char);
			System.Buffer.BlockCopy (buffer, at, chars, 0, size);
			at += size;

			s = new string (chars);
		}

		// int
		public void Write(int i){
			buffer [at++] = (byte)(i >> 24);
			buffer [at++] = (byte)(i >> 16);
			buffer [at++] = (byte)(i >> 8);
			buffer [at++] = (byte)(i);
		}

		public void Read(out int i){
			i = buffer [at++];
			i <<= 8;
			i |= buffer [at++];
			i <<= 8;
			i |= buffer [at++];
			i <<= 8;
			i |= buffer [at++];
		}

		// float
		public void Write(float f){
			byte[] bytes = System.BitConverter.GetBytes (f);
			buffer [at++] = bytes [0];
			buffer [at++] = bytes [1];
			buffer [at++] = bytes [2];
			buffer [at++] = bytes [3];
		}

		public void Read(out float f){
			f = System.BitConverter.ToSingle (buffer, at);
			at += 4;
		}
	}

	// config & channels
	GlobalConfig gcfg;
	ConnectionConfig concfg;
	int ReliableChannelId;
	int UnreliableChannelId;

	// other stuff
	int MaxConnections = 10;
	Connection[] connections;

	void Start(){
		Init ();

		Console.AddCommand ("print_buffer", (string arg) => {
			Console.Line ("Buffer size = " + buffer.at, Color.yellow);
			for (int i = 0; i < buffer.at; ++i) {
				Console.Line ("buffer[" + i + "] = " + buffer.buffer [i]);
			}
		});

		Console.AddCommand ("host", (string arg) => {
			StartHost(8888);
		});

		Console.AddCommand ("connect", (string arg) => {
			if(arg == string.Empty){
				const string defaultIP = "192.168.1.3";
				Connect(defaultIP, 8888);
			}else{
				Connect(arg, 8888);
			}
		});

		Console.AddCommand ("disconnect", (string arg) => {
			Disconnect();
		});

		Console.AddCommand ("ip", (string arg) => {
			Console.Line (Network.player.ipAddress, networkColor);
		});

		Console.AddCommand ("say", (string arg) => {
			buffer.WriteHeader(Message.Chat);
			buffer.Write(arg);

			switch(state){
			case State.Client:
				Cl_Send();
				break;
			case State.Server:
				Sv_Send();
				break;
			}
		});

		Console.AddCommand ("rename", (string arg) => {
			if(state == State.Client){
				const int NAME_MAX_LENGTH = 32;
				if(arg.Length > NAME_MAX_LENGTH){
					arg = arg.Remove(NAME_MAX_LENGTH);
				}

				buffer.WriteHeader(Message.PlayerRename);
				buffer.Write(arg);
				Cl_Send();
			}
		});
	}

	void Init(){
		const int bufferSize = 1024;
		buffer = new Buffer ();
		buffer.bufferSize = bufferSize;
		buffer.buffer = new byte[bufferSize];

		gcfg = new GlobalConfig ();
		NetworkTransport.Init (gcfg);

		concfg = new ConnectionConfig();

		ReliableChannelId = concfg.AddChannel (QosType.Reliable);
		UnreliableChannelId = concfg.AddChannel (QosType.StateUpdate);

		connections = new Connection[MaxConnections];

		Console.Line ("Networking interface initialized.", networkColor);
	}

	void StartHost(int port){
		if (state == State.None) {
			state = State.Server;

			HostTopology topology = new HostTopology (concfg, MaxConnections);
			hostId = NetworkTransport.AddHost (topology, port);

			Console.Line ("Host initialized", networkColor);
		}
	}

	void StartClient(){
		if (state == State.None) {
			state = State.Client;
			HostTopology topology = new HostTopology (concfg, 1);
			hostId = NetworkTransport.AddHost (topology);

			Console.Line ("Client initialized", networkColor);
		}
	}

	void Connect(string ip, int port){
		switch (state) {
		case State.None:
			StartClient ();
			goto case State.Client;
		case State.Client:
			connectionId = NetworkTransport.Connect (0, ip, port, 0, out error);
			Console.Line ("Connecting to " + ip + " ...", networkColor);
			break;
		}

		//NetworkTransport.Disconnect(hostId, connectionId, out error);
		//
	}

	void Cl_Send(){
		NetworkTransport.Send (hostId, connectionId, ReliableChannelId, buffer.buffer, buffer.at, out error);
	}

	void Cl_SendUnreliable(){
		NetworkTransport.Send (hostId, connectionId, UnreliableChannelId, buffer.buffer, buffer.at, out error);
	}

	void Sv_Send(){
		for (int i = 0; i < MaxConnections; ++i) {
			if (connections [i].UniquePlayerID > 0) {
				NetworkTransport.Send (hostId, i + 1, ReliableChannelId, buffer.buffer, buffer.at, out error);
			}
		}
	}

	void Sv_SendUnreliable(){
		for (int i = 0; i < MaxConnections; ++i) {
			if (connections [i].UniquePlayerID > 0) {
				NetworkTransport.Send (hostId, i + 1, UnreliableChannelId, buffer.buffer, buffer.at, out error);
			}
		}
	}

	void Sv_SendTo(int Connection){
		NetworkTransport.Send (hostId, Connection, ReliableChannelId, buffer.buffer, buffer.at, out error);
	}

	void Disconnect(){
		if (state == State.Client) {
			NetworkTransport.Disconnect (hostId, connectionId, out error);
		}
	}

	// Unreliable
	// TODO(lubomir): Currently we have a method named 'Sv_UnreliableSend' and a method 'Sv_SendUnreliable'
	// which might cause confusion. We should try to think of better names.
	void Sv_UnreliableSend(){
		buffer.at = 0;
		buffer.Write (ConnectedPlayers);

		for (int i = 0; i < MaxConnections; ++i) {
			if (connections [i].UniquePlayerID >= 0) {
				buffer.Write (i + 1);
				buffer.Write (connections [i].pos.x);
				buffer.Write (connections [i].pos.y);
				buffer.Write (connections [i].pos.z);
			}
		}

		Sv_SendUnreliable ();
	}

	void Cl_UnreliableSend(){
		Player p = Global.main.player;
		buffer.at = 0;

		buffer.Write (p.pos.x);
		buffer.Write (p.pos.y);
		buffer.Write (p.pos.z);

		Cl_SendUnreliable ();
	}

	void Sv_UnreliableReceive(){
		buffer.at = 0;

		buffer.Read(out connections [connectionId - 1].pos.x);
		buffer.Read(out connections [connectionId - 1].pos.y);
		buffer.Read(out connections [connectionId - 1].pos.z);
	}

	void Cl_UnreliableReceive(){
		buffer.at = 0;

		buffer.Read (out ConnectedPlayers);

		for (int i = 0; i < ConnectedPlayers; i++) {
			int id;
			buffer.Read (out id);
			buffer.Read (out connections [id - 1].pos.x);
			buffer.Read (out connections [id - 1].pos.y);
			buffer.Read (out connections [id - 1].pos.z);

			if (id != MyConnectionID && connections [id - 1].flare) {
				connections [id - 1].flare.localPosition = connections [id - 1].pos;
			}
		}
	}

	void Update()
	{
		// Sending data
		switch(state){
		case State.Client:
			Cl_UnreliableSend ();
			break;
		case State.Server:
			Sv_UnreliableSend();
			break;
		}

		// Receiving data
		NetworkEventType recData;
		do {
			recData = NetworkTransport.Receive (out recHostId, out connectionId, out channelId, buffer.buffer, buffer.bufferSize, out buffer.data, out error);

			// Reliable Channel
			if(channelId == ReliableChannelId){
				switch(state){
				// Client
				case State.Client:
					{
						switch (recData) {
						case NetworkEventType.Nothing:
							break;
						case NetworkEventType.ConnectEvent:
							Console.Line ("Connected!", networkColor);
							break;
						case NetworkEventType.DataEvent:
							Cl_Receive();
							break;
						case NetworkEventType.DisconnectEvent:
							NetworkError nerr = (NetworkError)error;
							Console.Line ("Disconnected. (" + nerr + ")", networkColor);
							break;
						}
					}
					break;
				// Server
				case State.Server:
					{
						switch (recData) {
						case NetworkEventType.Nothing:         //1
							break;
						case NetworkEventType.ConnectEvent:    //2
							ConnectedPlayers++;

							// TODO(lubomir): Separate server saves and loades UniquePlayerIDs and names
							connections[connectionId-1].UniquePlayerID = 1;
							connections[connectionId-1].name = "Player " + connectionId;
							Console.Line (connections[connectionId-1].name + " connected", networkColor);

							buffer.WriteHeader(Message.PlayerConnected);
							buffer.Write(connectionId);
							buffer.Write(connections[connectionId-1].name);
							Sv_Send();

							buffer.WriteHeader(Message.PlayerOnConnect);
							buffer.Write(connectionId);
							Sv_SendTo(connectionId);
							break;
						case NetworkEventType.DataEvent:       //3
							Sv_Receive ();
		//					Console.Line(connections[connectionId-1].name + " said:");
		//					Console.Line(buffer.ReadString());
		//					Console.Line(buffer.ReadString());
							break;
						case NetworkEventType.DisconnectEvent: //4
							ConnectedPlayers--;

							NetworkError nerr = (NetworkError)error;
							Console.Line (connections[connectionId-1].name + " disconnected (" + nerr + ")", networkColor);
							connections[connectionId-1].name = string.Empty;
							connections[connectionId-1].UniquePlayerID = 0;

							buffer.WriteHeader(Message.PlayerDisconnected);
							buffer.Write(connectionId);
							Sv_Send();
							break;
						}
					}
					break;
				}
			// Unreliable Channel
			}else{
				if (recData == NetworkEventType.DataEvent) {
					switch(state){
					case State.Client:
						Cl_UnreliableReceive();
						break;
					case State.Server:
						Sv_UnreliableReceive();
						break;
					}
				}
			}
		}while(recData > NetworkEventType.Nothing);
	}

	void Cl_Receive(){
		Message m = (Message)buffer.buffer [0];
		buffer.at = MESSAGE_SIZE;
		switch(m){
		case Message.PlayerConnected:
			{
				int ID;
				buffer.Read (out ID);
				buffer.Read (out connections [ID - 1].name);

				if (ID != MyConnectionID) {
					connections [ID - 1].flare = Pool.Request (RegisteredPool).transform;
				}
				Console.Line (connections [ID - 1].name + " connected", networkColor);
			}
			break;
		case Message.PlayerDisconnected:
			{
				int ID;
				buffer.Read (out ID);

				if (ID != MyConnectionID) {
					Pool.Release (connections [ID - 1].flare.gameObject);
				}
				Console.Line (connections [ID - 1].name + " disconnected", networkColor);
			}
			break;
		case Message.PlayerOnConnect:
			{
				buffer.Read (out MyConnectionID);
			}
			break;
		case Message.Chat:
			{
				string message;
				buffer.Read (out message);

				int player;
				buffer.Read (out player);

				Console.Line (connections [player - 1].name + ": " + message);
			}
			break;
		case Message.PlayerRename:
			{
				string name;
				buffer.Read (out name);

				int player;
				buffer.Read (out player);

				connections [player - 1].name = name;
			}
			break;
		case Message.ServerMessage:
			{
				string message;
				buffer.Read (out message);
				Console.Line (message, serverMessageColor);
			}
			break;
		}
	}

	public void Sv_Message(string message){
		if (state == State.Server) {
			buffer.WriteHeader (Message.ServerMessage);
			buffer.Write (message);
			Sv_Send ();
		}
	}

	void Sv_Receive(){
		Message m = (Message)buffer.buffer [0];
		buffer.at = MESSAGE_SIZE;
		switch(m){
		case Message.Chat:
			buffer.at = buffer.data;
			buffer.Write (connectionId);
			Sv_Send ();
			break;
		case Message.PlayerRename:
			string Original = connections [connectionId - 1].name;

			buffer.Read (out connections [connectionId - 1].name);
			buffer.at = buffer.data;
			buffer.Write (connectionId);
			Sv_Send ();

			Sv_Message (Original + " changed their name to " + connections [connectionId - 1].name);
			break;
		}
	}
}
