using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Security.Cryptography;
using UnityEngine.UI;
using Unity.VisualScripting;

using System.IO;
using System.Linq;

public class PlayerManager : NetworkBehaviour
{
    public GameObject Card1;
    public GameObject Card2;
    public GameObject PlayerArea;
    public GameObject EnemyArea;
    public GameObject DropZone;
    public Sprite newImage;

    List<GameObject> cards = new List<GameObject>();

    List<string> cardsNames = Directory.GetFiles(@"D:\Projetos\Unity\Helniv-Card-Game\Assets\Resources\CardSprites\front", "*.png", SearchOption.AllDirectories)
        .Select(Path.GetFileNameWithoutExtension).ToList();

    public override void OnStartClient()
    {
        base.OnStartClient();

        PlayerArea = GameObject.Find("PlayerArea");
        EnemyArea = GameObject.Find("EnemyArea");
        DropZone = GameObject.Find("DropZone");
    }

    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();

        for (int i = 0; i < 52; i++)
        {
            cards.Add(Card1);
        }
    }


    [Command]
    public void CmdDealCards()
    {
        //todo: adicionar validação de null para o final do deck
        GameObject card = Instantiate(cards[Random.Range(0, cards.Count)], new Vector2(0, 0), Quaternion.identity);

        var cardName = cardsNames[Random.Range(0, cardsNames.Count)];
        newImage = Resources.Load<Sprite>(@"CardSprites/front/" + cardName) as Sprite;
        card.GetComponent<Image>().sprite = newImage;

        cardsNames.Remove(cardName);
        cards.Remove(card);

        NetworkServer.Spawn(card, connectionToClient);

        RpcShowCard(card, "Dealt");
    }

    public void PlayCard(GameObject card)
    {
        CmdPlayCard(card);
    }

    [Command]
    void CmdPlayCard(GameObject card)
    {
        RpcShowCard(card, "Played");
    }

    [ClientRpc]
    void RpcShowCard(GameObject card, string type)
    {
        Debug.Log(type);
        if (type == "Dealt")
        {
            if (hasAuthority)
            {
                card.transform.SetParent(PlayerArea.transform, false);
            }
            else
            {
                card.transform.SetParent(EnemyArea.transform, false);
                card.GetComponent<CardFlipper>().Flip();
            }
        }
        else if (type.Equals("Played"))
        {
            card.transform.SetParent(DropZone.transform, false);
            if (!hasAuthority)
            {
                card.GetComponent<CardFlipper>().Flip();
            }
        }
    }

    [Command]
    public void CmdTargetSelfCard()
    {
        TargetSelfCard();
    }

    [Command]
    public void CmdTargerOtherCard(GameObject target)
    {
        NetworkIdentity opponentIdentity = target.GetComponent<NetworkIdentity>();
        TargetOtherCard(opponentIdentity.connectionToClient);
    }

    [TargetRpc]
    void TargetSelfCard()
    {
        Debug.Log("Targeted by self");
    }

    [TargetRpc]
    void TargetOtherCard(NetworkConnection target)
    {
        Debug.Log("Targeted by Other");
    }

}
