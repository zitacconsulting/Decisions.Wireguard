using System.Runtime.Serialization;

namespace Zitac.Wireguard.Steps;

[DataContract]
public class WireguardKeyPair
{

    [DataMember]
    public string? PublicKey { get; set; }

    [DataMember]
    public string? PrivateKey { get; set; }

    public WireguardKeyPair(){}
    public WireguardKeyPair(string publicKey, string privateKey)
    {
        this.PublicKey = publicKey;
        this.PrivateKey = privateKey;
    }

}