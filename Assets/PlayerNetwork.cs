using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{

    NetworkVariable<Vector3> unitPosition = new NetworkVariable<Vector3>(new Vector3(0,0,0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
       new MyCustomData{
            _int = 56,
            _bool = true,
       }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };

        unitPosition.OnValueChanged += (Vector3 previousValue, Vector3 newValue) => {
            transform.position += newValue;
        };
    }


    void Update()
    {
        
        if(!IsOwner) return;

        if(Input.GetKeyDown(KeyCode.T))
        {
            TestServerRpc(new ServerRpcParams());
            /*
            randomNumber.Value = new MyCustomData {
                _int = 10,
                _bool = false,
                message = "Kowabunga!",
            };
            */
        }

        Vector3 moveDir = new Vector3(0,0);
        if(Input.GetKey(KeyCode.W)) moveDir.y = +1f;
        if(Input.GetKey(KeyCode.S)) moveDir.y = -1f;
        if(Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if(Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
        unitPosition.Value = moveDir * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams) {
        Debug.Log("TestSErverRpc" + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId); 
    }

}

