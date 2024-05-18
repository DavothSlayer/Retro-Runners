using UnityEngine;
using TMPro;

namespace RetroCode
{
    public class FramerateDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        private void Update()
        {
            int fps = Mathf.RoundToInt(1.0f / Time.smoothDeltaTime);

            text.text = fps.ToString();
        }
    }
}
