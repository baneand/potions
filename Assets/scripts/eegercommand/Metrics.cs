using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// TODO: Use strategy pattern to eliminate switch statement and
// convert lastRewards() into lastReward(int strandNumber)
// Also convert rewardCount() into rewardCount(int strandNumber)

public sealed class Metrics {
	private List<DateTime> strand0Rewards = new List<DateTime>();
	private List<DateTime> strand1Rewards = new List<DateTime>();
	private List<DateTime> strand2Rewards = new List<DateTime>();
	private List<DateTime> strand3Rewards = new List<DateTime>();
	private static Metrics instance = null;
		
	public static Metrics Instance()	{
		if (instance == null) {
			instance = new Metrics();
		}
		
		return instance;
	}

	public List<DateTime> lastRewards() {
		List<DateTime> lastRewards = new List<DateTime>();

		lastRewards.Add(strand0Rewards.DefaultIfEmpty(DateTime.MinValue).Last ());
		lastRewards.Add(strand1Rewards.DefaultIfEmpty(DateTime.MinValue).Last ());
		lastRewards.Add(strand2Rewards.DefaultIfEmpty(DateTime.MinValue).Last ());
		lastRewards.Add(strand3Rewards.DefaultIfEmpty(DateTime.MinValue).Last ());

		return lastRewards;
	}

	public List<int> rewardCounts() {
		List<int> counts = new List<int>();

		counts.Add (strand0Rewards.Count());
		counts.Add (strand1Rewards.Count());
		counts.Add (strand2Rewards.Count());
		counts.Add (strand3Rewards.Count());

		return counts;
	}

	public void logReward(int code) {
		DateTime timestamp = DateTime.Now;

		switch (code) {
		case 0:
			strand0Rewards.Add (timestamp);
			break;
		case 1:
			strand1Rewards.Add (timestamp);
			break;
		case 2:
			strand2Rewards.Add (timestamp);
			break;
		case 3:
			strand3Rewards.Add (timestamp);
			break;
		default:
			throw new System.Exception ("Unexpected reward code: " + code);
		}
	}
}
