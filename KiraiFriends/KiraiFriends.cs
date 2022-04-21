using MelonLoader;
using UnityEngine;
using VRC.Core;

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
            ///UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/Favorite Avatar List
            Transform screens = GameObject.Find("MenuContent/Screens").transform;
            // generate menu

            Transform social = screens.Find("Social/Vertical Scroll View/Viewport/Content");
            Transform avalist = screens.Find("Avatar/Vertical Scroll View/Viewport/Content/Favorite Avatar List");
            Transform onlineFriends = social.Find("OnlineFriends");

            Transform kiraiFriends = Object.Instantiate(avalist, onlineFriends.parent);
            kiraiFriends.gameObject.name = "KiraiFriends";
            kiraiFriends.SetSiblingIndex(onlineFriends.GetSiblingIndex() + 1);

            UnityEngine.UI.LayoutElement layout = kiraiFriends.GetComponent<UnityEngine.UI.LayoutElement>();
            layout.minWidth = -1;
            layout.minHeight = 250;

            Transform button = kiraiFriends.Find("Button");
            button.Find("TitleText").GetComponent<UnityEngine.UI.Text>().text = "Kirai Friends";

            //Transform viewport = kiraiFriends.Find("ViewPort");
            //viewport.localPosition = new Vector3(200, 82, 0);

            //RectTransform rect = viewport.GetComponent<RectTransform>();
            //rect.sizeDelta = new Vector2(690, -58);

            //Transform content = viewport.Find("Content");

            //for (int i = 0; i < content.childCount; i++)
            //    Object.Destroy(content.GetChild(i));

            //var target = social.Find("InRoom/ViewPort/Content/UserUiPrefab(Clone) 1");

            //for (int i = 0; i < 5; i++)
            //{
            //    var temp = Object.Instantiate(target, content);
            //    temp.Find("Icons").gameObject.active = false;
            //    temp.Find("Background/ElementImageShape/ElementImage/Panel/TitleText").GetComponent<UnityEngine.UI.Text>().text = i.ToString();
            //}
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
