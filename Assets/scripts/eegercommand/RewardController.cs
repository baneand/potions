using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class RewardController : MonoBehaviour {
	public Text messageText;

	const int messagesToDisplay = 30;
	
	void Update () {
		//List<DateTime> rewards = Metrics.Instance ().lastRewards ();
		//List<int> counts = Metrics.Instance ().rewardCounts ();
		//string message = string.Format ("REW0: ({0}) {1}\nREW1: ({2}) {3}\nREW2: ({4}) {5}\nREW3: ({6}) {7}",
		//                                counts[0], rewards[0],
		//                                counts[1], rewards[1],
		//                                counts[2], rewards[2],
		//                                counts[3], rewards[3]);
		//messageText.text = message;
	}
} 