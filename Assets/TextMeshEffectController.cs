using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextMeshEffectController : MonoBehaviour
{
    public TMP_Text textMesh;
    public TextMesh textMesh2;

    Mesh mesh;

    Vector3[] vertices;

    public float strength = 1f;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (textMesh != null)
        {
            textMesh.ForceMeshUpdate();
            mesh = textMesh.mesh;
            vertices = mesh.vertices;

            for (int i = 0; i < textMesh.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo c = textMesh.textInfo.characterInfo[i];

                int index = c.vertexIndex;

                Vector3 offset = Wobble(Time.time + i) * strength;
                vertices[index] += offset;
                vertices[index + 1] += offset;
                vertices[index + 2] += offset;
                vertices[index + 3] += offset;
            }

            mesh.vertices = vertices;
            textMesh.canvasRenderer.SetMesh(mesh);
        }

        if (textMesh2 != null)
        {
            // textMesh2.transform.DOShakePosition(1f, 0.9f);
        }
    }

    Vector2 Wobble(float time) {
        return new Vector2(Mathf.Sin(time*3.3f), Mathf.Cos(time*2.5f));
    }
}
