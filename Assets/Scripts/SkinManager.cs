using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct SkinSet
{
    public Sprite bodySkin;
    public Sprite[] handSkins;
}

public class SkinManager : MonoBehaviour
{
    public Sprite[] faceSkins;
    public SkinSet[] skinSets;

    public int userSkin = 0;
    public int userHand = 0;

    public SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer[] handRenderers;
    [SerializeField] private SpriteRenderer faceRenderer;

    private GameMananger gameManager;

    void Awake()
    {
        gameManager = GameMananger.GameInstance;

        userHand = 5;
        userSkin = gameManager.usedSkin;
        gameManager.usedSkin++;

        Debug.Log("Skin: " + userSkin);

        faceRenderer.sprite = faceSkins[Random.Range(0, faceSkins.Length)];
    }

    void Update()
    {
        SetSkins(userSkin);
        SetHand(userHand);
    }

    void SetSkins(int skin)
    {
        spriteRenderer.sprite = skinSets[skin].bodySkin;

        for (int i = 0; i < handRenderers.Length; i++)
        {
            handRenderers[i].sprite = skinSets[skin].handSkins[i];
        }
    }

    public void SetHand(int hand)
    {
        for (int i = 0; i < handRenderers.Length; i++)
        {
            handRenderers[i].sprite = skinSets[userSkin].handSkins[hand];
        }
    }
}
