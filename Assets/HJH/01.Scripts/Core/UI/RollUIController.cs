using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RollUIController : MonoBehaviour
{
    [SerializeField] private Image _rollFillImage;
    [SerializeField] private TextMeshProUGUI _rollCooldownTxt;


    public void UpdateRollUI(float remaining, float total)
    {
        float fillAmount = total > 0f ? 1f - (remaining / total) : 1f;
        fillAmount = Mathf.Clamp01(fillAmount);

        _rollFillImage.fillAmount = fillAmount;

        if (remaining <= 0f)
        {
            _rollCooldownTxt.text = "";
            _rollCooldownTxt.gameObject.SetActive(false);
        }
        else
        {
            if (!_rollCooldownTxt.gameObject.activeSelf)
            {
                _rollCooldownTxt.gameObject.SetActive(true);
            }

            _rollCooldownTxt.text = remaining.ToString("F1");
        }
    }
}