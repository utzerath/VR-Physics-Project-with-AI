using System.Collections.Generic;
using Inworld.Packets;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Inworld.Util
{
    public class TestTrigger : MonoBehaviour
    {
        [SerializeField] InworldCharacter Character;
        public string trigger;
        public string triggerResponse;
        public UnityEvent OnTriggerSent;
        public UnityEvent OnTriggerReceived;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter(Collider other)
        {
            Renderer render = GetComponent<Renderer>();
            render.material.color = Color.green;
            SendTrigger(trigger);
        }
        public void SendTrigger(string triggerName)
        {
            string[] triggerArray = triggerName.Split("triggers/");
            SendEventToAgent(triggerArray.Length == 2 ? new CustomEvent(triggerArray[1]) : new CustomEvent(triggerName));
        }
        public void SendEventToAgent(InworldPacket packet)
        {
            //need to confirm what id to use for scene triggers
            string ID = Character ? Character.ID : InworldController.CurrentScene.name;
            packet.Routing = Routing.FromPlayerToAgent(ID);
            InworldController.Instance.SendEvent(packet);
        }

    }
}
