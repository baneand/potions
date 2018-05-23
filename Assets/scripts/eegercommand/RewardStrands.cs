using System.Collections.Generic;
using LitJson;

public class RewardStrands {
	List<RewardableState> values = new List<RewardableState> ();

	public RewardStrands(JsonData json)
    {
        if(json == null || !json.IsArray)
        {
            return;
        }
        for(int i = 0; i < json.Count; i ++)
        {
            JsonData strandJSON = json[i];
            JsonData rewardableStateJSON = strandJSON[0];
            int reward = JsonUtil.ParseInteger(strandJSON[1]);
            add(new RewardableState(rewardableStateJSON, reward));
        }
	}
	
	public void add(RewardableState value) {
		values.Add (value);
	}
	
	public void debug() {
		foreach (RewardableState value in values) {
			value.debug ();
		}
	}
}
