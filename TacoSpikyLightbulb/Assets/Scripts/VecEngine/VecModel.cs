using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VecModel : MonoBehaviour
{
    public TextAsset VecMeshAsset; // The VecMesh .txt file.

    private List<VecMesh> VecMeshes;

    public VecMesh.MaskedBy MaskedBy = VecMesh.MaskedBy.All;

    // Start is called before the first frame update
    void Start()
    {
        // Hide the original MeshFilter (it was just a placeholder).
        Destroy(this.gameObject.GetComponent<MeshFilter>());

        if (VecMeshAsset == null)
            throw new System.Exception("VecModel must have at least one VecMeshAsset.");

        ProcessVecMeshAssets();
    }

    private void ProcessVecMeshAssets()
    {
        VecMeshes = new List<VecMesh>();
        
        VecMeshProcessor.VecMeshData[] vmds = VecMeshProcessor.ProcessTextAsset(VecMeshAsset);
        foreach(var vmd in vmds) {

            // Create the GameObject.
            var meshGO = new GameObject("VecMesh");
            VecMesh vm = meshGO.AddComponent<VecMesh>() as VecMesh;
            vm._MaskedBy = MaskedBy;
            vm.MeshData = vmd;

            // Parent to this gameobject.
            meshGO.transform.parent = this.gameObject.transform;

            VecMeshes.Add(vm);
        }

        //vecMeshes = _vecMeshes.ToArray();
    }

    public void Explode()
    {
        foreach(var e in VecMeshes)
            e.Explode();

        VecMeshes.Clear();
        Destroy(this);
        //Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
