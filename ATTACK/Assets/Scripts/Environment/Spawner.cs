using UnityEngine;
using UnityEngine.Rendering;

public class Spawner : MonoBehaviour
{
    GameObject SpawnedCharacter;
    bool Spawning = false;
    const float TotalSpawnTime = 0.75f;
    float CurrentTime = 0;

    Vector3 MaxScale = Vector3.zero;
    private void Start()
    {
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

    public GameObject Spawn(Character character)
    {
        Team team = GameManager.GetTeamFromTag(tag);

        Quaternion towardsMiddle = new Quaternion(0, (int)team * 180, 0, 1);
        SpawnedCharacter = Instantiate(character.GetModelPrefab(), transform);
        SpawnedCharacter.transform.localRotation = towardsMiddle;
        SpawnedCharacter.GetComponent<CharacterCommon>().SetTeam(team);
        SpawnedCharacter.SetActive(false);

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
