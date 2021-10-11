using UnityEngine;
using UnityEngine.Rendering;

public class Spawner : MonoBehaviour
{
    GameObject SpawnedCharacter;
    bool Spawning = false;
    const float TotalSpawnTime = 1f;
    float CurrentTime = 0;

    private MaterialPropertyBlock Mpb;

    Vector3 MaxScale = Vector3.zero;
    private void Awake()
    {
        Mpb = new MaterialPropertyBlock();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.name.Contains("SpawnBulb"))
            {
                MaxScale = child.transform.localScale;
                child.transform.localScale = 0 * MaxScale;
            }
        }
    }

    void Update()
    {
        if(Spawning)
        {
            CurrentTime += Time.deltaTime;
            if (CurrentTime > TotalSpawnTime)
            {
                Spawning = false;
                CurrentTime = 0;
                SetSpawnBulbSize(0);
                return;
            }

            float t = CurrentTime / TotalSpawnTime;
            t = Mathf.Sin(t * Mathf.PI);
            if (t > 0.5 && SpawnedCharacter != null && !SpawnedCharacter.activeSelf)
                SpawnedCharacter.SetActive(true);
            SetSpawnBulbSize(t);
        }
    }

    public GameObject Spawn(Character character, ServerHandler.Suit suit)
    {
        if (suit == ServerHandler.Suit.SPADES)
        {
            Mpb.SetColor("_CoreColor", new Color(0.36f, 0.36f, 0.67f));
            Mpb.SetColor("_OuterSwirlColor", new Color(0.04f, 0.23f, 0.30f));
            Mpb.SetColor("_InnerSwirlColor", new Color(0f, 0.13f, 0.42f));
        }
        else if (suit == ServerHandler.Suit.CLUBS)
        {
            Mpb.SetColor("_CoreColor", new Color(0.44f, 0.19f, 0.21f));
            Mpb.SetColor("_OuterSwirlColor", new Color(0.67f, 0.25f, 0f));
            Mpb.SetColor("_InnerSwirlColor", new Color(0.88f, 0.19f, 0f));
        }
        else
        {
            Debug.LogError("Non implemented suit used for Spawner");
        }
        
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.tag.Equals("SpawnBulb"))
            {
                child.GetComponent<Renderer>().SetPropertyBlock(Mpb);
            }
        }

        Team team = GameManager.GetTeamFromTag(tag);
        Quaternion towardsMiddle = new Quaternion(0, (int)team * 180, 0, 1);
        SpawnedCharacter = Instantiate(character.GetModelPrefab(), transform);
        SpawnedCharacter.transform.localRotation = towardsMiddle;
        SpawnedCharacter.GetComponent<CharacterCommon>().SetTeam(team);
        SpawnedCharacter.SetActive(false);

        GetComponent<AudioSource>().Play();

        Spawning = true;

        return SpawnedCharacter;
    }

    private void SetSpawnBulbSize(float t)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.name.Contains("SpawnBulb"))
            {
                child.transform.localScale = t * MaxScale;
            }
        }
    }
}
