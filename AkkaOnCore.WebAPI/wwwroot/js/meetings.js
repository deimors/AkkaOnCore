﻿"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("https://localhost:44333/meetingsHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("OnMeetingAddedToList", function (id, name) {
	var li = document.createElement("li");
	li.textContent = name;
	document.getElementById("messagesList").appendChild(li);
});

connection.start().then(function () {
	document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
	return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
	var user = document.getElementById("userInput").value;
	var message = document.getElementById("messageInput").value;
	connection.invoke("SendMessage", user, message).catch(function (err) {
		return console.error(err.toString());
	});
	event.preventDefault();
});