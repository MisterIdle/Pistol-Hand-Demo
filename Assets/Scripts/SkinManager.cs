using UnityEngine;

[System.Serializable]
public struct SkinSet
{
    public Sprite bodySkin;
    public Sprite[] handSkins;
}

public class SkinManager : MonoBehaviour
{
    public Sprite noPlayer;
    public Sprite[] faceSkins;
    public SkinSet[] skinSets;

    public int userSkin = 0;
    public int userHand = 1;
    public int userHead = 0;

    private PlayersController playerController;

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer[] handRenderers;
    public SpriteRenderer faceRenderer;

    public HUDManager hudManager;

    void Awake()
    {
        playerController = GetComponent<PlayersController>();
        spriteRenderer = playerController.GetComponent<SpriteRenderer>();
        handRenderers = playerController.hand.GetComponentsInChildren<SpriteRenderer>();
        faceRenderer = playerController.face.GetComponent<SpriteRenderer>();

        hudManager = FindObjectOfType<HUDManager>();

        userSkin = playerController.playerID;
        hudManager.skins[playerController.playerID].sprite = SetSkinHUD(userSkin);

        int randomFace = Random.Range(0, faceSkins.Length - 2);

        hudManager.face[playerController.playerID].sprite = faceSkins[randomFace];
        faceRenderer.sprite = faceSkins[randomFace];

        userHead = randomFace;

        hudManager.lives[playerController.playerID].color = Color.green;
    }

    void Update()
    {
        SetSkins(userSkin);
        SetHand(userHand);
        UpdateLifeColor();
    }

    public void UpdateLifeColor()
    {
        foreach (PlayersController player in FindObjectsOfType<PlayersController>())
        {
            switch (player.lifes)
            {
                case 3:
                    hudManager.lives[player.playerID].color = Color.green;
                    break;
                case 2:
                    hudManager.lives[player.playerID].color = Color.yellow;
                    break;
                case 1:
                    hudManager.lives[player.playerID].color = Color.red;
                    break;
                case 0:
                    hudManager.lives[player.playerID].color = Color.black;
                    break;
            }
        }
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
