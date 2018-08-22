using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using LeanCloud.Play;

public class PlayQuickStart : MonoBehaviour {
    public Text idText = null;
    public Text scoreText = null;
    public Text resultText = null;

    private Play play = Play.Instantce;

	// Use this for initialization
	void Start () {
        LeanCloud.Play.Logger.LogDelegate = (level, log) =>
        {
            if (level == LogLevel.Debug) {
                Debug.LogFormat("[DEBUG] {0}", log);
            } else if (level == LogLevel.Warn) {
                Debug.LogFormat("[WARN] {0}", log);
            } else if (level == LogLevel.Error) {
                Debug.LogFormat("[ERROR] {0}", log);
            }
        };

        var now = System.DateTime.Now;
        var roomName = string.Format("{0}_{1}", now.Hour, now.Minute);
        var random = new System.Random();
        var randId = string.Format("{0}", random.Next(10000000));
        play.Init("315XFAYyIGPbd98vHPCBnLre-9Nh9j0Va", "Y04sM6TzhMSBmCMkwfI3FpHc", Region.EastChina);
        play.UserId = randId;
        this.idText.text = string.Format("Id: {0}", randId);

        play.On(LeanCloud.Play.Event.CONNECTED, (evtData) =>
        {
            Debug.Log("connected");
            play.JoinOrCreateRoom(roomName);
        });
        play.On(LeanCloud.Play.Event.ROOM_JOINED, (evtData) =>
        {
            Debug.Log("joined room");
        });
        play.On(LeanCloud.Play.Event.PLAYER_ROOM_JOINED, (evtData) =>
        {
            var newPlayer = evtData["newPlayer"] as Player;
            Debug.LogFormat("new player: {0}", newPlayer.UserId);
            if (play.Player.IsMaster) {
                var playerList = play.Room.PlayerList;
                for (int i = 0; i < playerList.Count; i++) {
                    var player = playerList[i];
                    var props = new Dictionary<string, object>();
                    if (player.IsMaster) {
                        props.Add("point", 10);
                    } else {
                        props.Add("point", 5);
                    }
                    player.SetCustomProperties(props);
                }
                var data = new Dictionary<string, object>();
                data.Add("winnerId", play.Room.Master.ActorId);
                var opts = new SendEventOptions();
                opts.ReceiverGroup = ReceiverGroup.All;
                play.SendEvent("win", data, opts);
            }
        });
        play.On(LeanCloud.Play.Event.PLAYER_CUSTOM_PROPERTIES_CHANGED, (evtData) => {
            var player = evtData["player"] as Player;
            if (player.IsLocal) {
                long point = (long)player.CustomProperties["point"];
                Debug.LogFormat("{0} : {1}", player.UserId, point);
                this.scoreText.text = string.Format("Score: {0}", point);
            }
        });
        play.On(LeanCloud.Play.Event.CUSTOM_EVENT, (evtData) =>
        {
            var eventId = evtData["eventId"] as string;
            if (eventId == "win") {
                var eventData = evtData["eventData"] as Dictionary<string, object>;
                int winnerId = (int)(long)eventData["winnerId"];
                if (play.Player.ActorId == winnerId) {
                    Debug.Log("win");
                    this.resultText.text = "Win";
                } else {
                    Debug.Log("lose");
                    this.resultText.text = "Lose";
                }
                play.Disconnect();
            }
        });

        play.Connect();
	}
	
	// Update is called once per frame
	void Update () {
        play.HandleMessage();
	}
}
