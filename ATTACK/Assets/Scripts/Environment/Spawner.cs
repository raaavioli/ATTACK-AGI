using UnityEngine;
using UnityEngine.Rendering;

public class Spawner : MonoBehaviour
{
    public GameObject SpawnBulb;

    GameObject SpawnedCharacter;
    bool Spawning = false;
    const float TotalSpawnTime = 1f;
    float CurrentTime = 0;

    private MaterialPropertyBlock Mpb;

    Vector3 MaxScale = Vector3.zero;
    private void Awake()
    {
        Mpb = new MaterialPropertyBlock();
        MaxScale = SpawnBulb.transform.localScale;
        SpawnBulb.transform.localScale = 0 * MaxScale;
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

        foreach (GlowUpdater gu in GetComponentsInChildren<GlowUpdater>())
        {
            if (SpawnedCharacter != null)
            {
                CharacterMode mode = SpawnedCharacter.GetComponent<CharacterCommon>().Mode;
                if (mode == CharacterMode.Offensive)
                    gu._Color = Colors.AttackColor;
                else if (mode == CharacterMode.Defensive)
                    gu._Color = Colors.DefenceColor;
            } else
            {
                gu._Color = Color.clear;
            }
        }
    }

    public GameObject Spawn(Character character, CharacterMode mode)
    {
        if (mode == CharacterMode.Defensive)
        {
            Mpb.SetColor("_CoreColor", Colors.LightPurple);
            Mpb.SetColor("_OuterSwirlColor", Colors.DarkCyan);
            Mpb.SetColor("_InnerSwirlColor", Colors.DarkBlue);
        }
        else if (mode == CharacterMode.Offensive)
        {
            Mpb.SetColor("_CoreColor", Colors.WineRed);
            Mpb.SetColor("_OuterSwirlColor", Colors.DarkOrange);
            Mpb.SetColor("_InnerSwirlColor", Colors.FireOrange);
        }
        else
        {
            Debug.LogError("Non implemented suit used for Spawner");
        }

        SpawnBulb.GetComponent<Renderer>().SetPropertyBlock(Mpb);

        Team team = GameManager.GetTeamFromTag(tag);
        Quaternion towardsMiddle = new Quaternion(0, (int)team * 180, 0, 1);
        SpawnedCharacter = Instantiate(character.GetModelPrefab(), transform);
        SpawnedCharacter.transform.localRotation = towardsMiddle;
        SpawnedCharacter.GetComponent<CharacterCommon>().SetTeam(team);
        SpawnedCharacter.GetComponent<CharacterCommon>().Mode = mode;
        SpawnedCharacter.SetActive(false);

        GetComponent<AudioSource>().Play();

        Spawning = true;

        return SpawnedCharacter;
    }

    private void SetSpawnBulbSize(float size)
    {
        SpawnBulb.transform.localScale = size * MaxScale;
    }
}
