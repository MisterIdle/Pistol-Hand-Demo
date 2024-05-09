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

    private PlayersController playerController;

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer[] handRenderers;
    public SpriteRenderer faceRenderer;

    public HUDManager hudManager;

    void Start()
    {
        playerController = GetComponent<PlayersController>();
        spriteRenderer = playerController.GetComponent<SpriteRenderer>();
        handRenderers = playerController.hand.GetComponentsInChildren<SpriteRenderer>();
        faceRenderer = playerController.face.GetComponent<SpriteRenderer>();

        hudManager = FindObjectOfType<HUDManager>();

        userSkin = playerController.playerID;
        hudManager.skins[playerController.playerID].sprite = SetSkinHUD(userSkin);

        int randomFace = Random.Range(0, faceSkins.Length);

        hudManager.face[playerController.playerID].sprite = faceSkins[randomFace];
        hudManager.face[playerController.playerID].color = new Color(1, 1, 1, 1);
        faceRenderer.sprite = faceSkins[randomFace];

    }

    void Update()
    {
        SetSkins(userSkin);
        SetHand(userHand);
    }

    public Sprite SetSkins(int skin)
    {
        spriteRenderer.sprite = skinSets[skin].bodySkin;
        return spriteRenderer.sprite;
    }

    public Sprite SetSkinHUD(int skin)
    {
        return skinSets[skin].bodySkin;
    }

    public void SetHand(int hand)
    {
        for (int i = 0; i < handRenderers.Length; i++)
        {
            handRenderers[i].sprite = skinSets[userSkin].handSkins[hand];
        }
    }
}
