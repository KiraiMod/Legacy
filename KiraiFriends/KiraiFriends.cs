using MelonLoader;
using UnityEngine;

namespace KiraiMod
{
    public class KiraiFriends : MelonMod
    {
        public override void OnApplicationStart()
        {
            // get friends list

            MelonCoroutines.Start(KeepAlive());
        }

        public override void VRChat_OnUiManagerInit()
        {
            // generate menu

            Transform social = GameObject.Find("MenuContent/Screens/Social/Vertical Scroll View/Viewport/Content").transform;
            Transform onlineFriends = social.Find("OnlineFriends");

            Transform kiraiFriends = Object.Instantiate(onlineFriends, onlineFriends.parent);
            kiraiFriends.SetSiblingIndex(onlineFriends.GetSiblingIndex() + 1);

            Transform button = kiraiFriends.Find("Button");
            button.Find("TitleText").GetComponent<UnityEngine.UI.Text>().text = "Kirai Friends";

            Transform content = kiraiFriends.Find("ViewPort/Content");

            Object.Destroy(kiraiFriends.GetComponent<UiUserList>());

            for (int i = 0; i < content.childCount; i++)
                Object.Destroy(content.GetChild(i));

            var target = social.Find("InRoom/ViewPort/Content/UserUiPrefab(Clone) 1");

            for (int i = 0; i < 5; i++)
            {
                var temp = Object.Instantiate(target, content);
                temp.Find("Icons").gameObject.active = false;
                temp.Find("Background/ElementImageShape/ElementImage/Panel/TitleText").GetComponent<UnityEngine.UI.Text>().text = i.ToString();
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Q)) VRChat_OnUiManagerInit();
        }

        private System.Collections.IEnumerator KeepAlive()
        {
            // do a fuckin keep alive
            yield return new WaitForSeconds(5);
        }
    }
}
