using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DoomtrinityFPSPrototype.Utils {
	
	public class BaseMessage {
		public string name;
		public BaseMessage() { name = this.GetType().Name; }
	}

	public delegate bool MessageHandlerDelegate(BaseMessage message);

	// A powerful messaging system class that allows to send messages to all listeners when a specific event occurs.
	// This is great to keep the code decoupled.
	// You basically have to do few things to get this work:
	// 1 - define your custom message that derives from 'BaseMessage'; this is the message that you need to fire with 
	//     'QueueMessage' when a particular event occurs;
	// 2 - define a method with a signature that matches the 'MessageHandlerDelegate' delegate, in the specific script component
	//     that should do something when that event occurs;
	// 3 - attach the listener, so the messaging system will call the defined method when the event occurs; detach the listener
	//     when you no longer need it ( usually attach it in 'Start', and detach it in 'OnDestroy' ).
	// Note: if a listner returns true, the event notification will stop for this specific message, 
	// so the message won't propagate for the remaining listners to process.
	// Take a look to the 'AmmoQtyChangeMessage' message in 'Weapon' script.
	public class MessagingSystem : SingletonAsComponent<MessagingSystem> {

		// ========================================================================================================================
		// Instance variables

		private Dictionary<string,List<MessageHandlerDelegate>> _listenerDict = new Dictionary<string,List<MessageHandlerDelegate>>();		
		private Queue<BaseMessage> _messageQueue = new Queue<BaseMessage>();	
		private int maxQueueProcessingCount = 10;

		// ========================================================================================================================
		// Singleton stuff


		public static MessagingSystem Instance {
			get { return ((MessagingSystem)_Instance); }
			set { _Instance = value; }
		}

		// ========================================================================================================================
		// AttachListener

		public bool AttachListener(System.Type type, MessageHandlerDelegate handler) {
			if (type == null) 
			{
				Debug.Log("MessagingSystem: AttachListener failed due to no message type specified");
				return false;
			}

			string msgName = type.Name;

			if (!_listenerDict.ContainsKey(msgName)) {
				_listenerDict.Add(msgName, new List<MessageHandlerDelegate>());
			}

			List<MessageHandlerDelegate> listenerList = _listenerDict[msgName];
			if (listenerList.Contains(handler))
			{
				return false; // Listener already in list.
			}

			listenerList.Add(handler);
			return true;
		}

		// ========================================================================================================================
		// QueueMessage

		public bool QueueMessage(BaseMessage msg)
		{
			if (!_listenerDict.ContainsKey(msg.name))
			{
				return false;
			}
			_messageQueue.Enqueue(msg);
			return true;
		}

		// ========================================================================================================================
		// Update

		private void Update()
		{
			float queue_count = 0.0f;
			while (_messageQueue.Count > 0) {
				if (maxQueueProcessingCount > 0) {
					if (queue_count > maxQueueProcessingCount)
						return;
				}

				BaseMessage msg = _messageQueue.Dequeue();
				if (!TriggerMessage(msg))
					Debug.Log("Error when processing message: " + msg.name);

				if (maxQueueProcessingCount > 0)
					queue_count++;
			}
		}

		// ========================================================================================================================
		// TriggerMessage

		public bool TriggerMessage(BaseMessage msg) {
			string msgName = msg.name;
			if (!_listenerDict.ContainsKey(msgName))
			{
				Debug.Log("MessagingSystem: Message \"" + msgName + "\" has no listeners!");
				return false; // No listeners for this message so ignore it.
			}

			List<MessageHandlerDelegate> listenerList = _listenerDict[msgName];

			for(int i = 0; i < listenerList.Count; ++i) {
				if (listenerList[i](msg))
					return true; // Message consumed.
			}

			return true;
		}

		// ========================================================================================================================
		// DetachListener

		public bool DetachListener(System.Type type, MessageHandlerDelegate handler)
		{
			if (type == null) 
			{
				Debug.Log("MessagingSystem: DetachListener failed due to no message type specified");
				return false;
			}

			string msgName = type.Name;

			if (!_listenerDict.ContainsKey(type.Name)) {
				return false;
			}

			List<MessageHandlerDelegate> listenerList = _listenerDict[msgName];
			if (!listenerList.Contains (handler)) {
				return false;
			}

			listenerList.Remove(handler);
			return true;
		}

		// ========================================================================================================================
	}
}