RTSDK C#
RTNetwork.cs: Includes general functions to create network sockets (both UDP and TCP), connecting to QTM server, send data over TCP, receiving data over TCP and UDP and also sending UDP broadcast message. 

RTProtocol.cs: Simplifies sending commands to QTM and also receiving data from QTM. It has two callback delegates, realTimeDataCallback and eventDataCallback. Functions registered to this callback will be called after you call listenToStream(). To stop listening, StopStreamListen() can be called.

RTPacket.cs: Class for parsing packets received from QTM.
BitConver.cs: Class converts bytes to data types.
