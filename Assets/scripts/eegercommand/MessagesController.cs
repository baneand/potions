using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class MessagesController : MonoBehaviour {
	public Text messageText;
	private List<string> buffer = new List<string>();
	const int messagesToDisplay = 5;
	
	public void add(string message) {
		buffer.Add (message);
	}

	void Update () {
		int count = buffer.Count;
		int offset = count > messagesToDisplay ? count - messagesToDisplay : 0;
		int display = count < messagesToDisplay ? count : messagesToDisplay;
		messageText.text = string.Join ("\n", buffer.GetRange (offset, display).ToArray());
	}
}