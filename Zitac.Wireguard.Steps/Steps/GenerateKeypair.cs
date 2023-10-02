using System;
using DecisionsFramework.Design.Flow;
using DecisionsFramework.Design.ConfigurationStorage.Attributes;
using DecisionsFramework.Design.Flow.CoreSteps;
using DecisionsFramework.Design.Flow.Mapping;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;

namespace Zitac.Wireguard.Steps;

[AutoRegisterStep("Generate Keypair", "Integration", "Wireguard")]
[Writable]
public class GenerateWireguardKeyPair : BaseFlowAwareStep, ISyncStep, IDataProducer
{
    public override OutcomeScenarioData[] OutcomeScenarios
    {
        get
        {
            List<OutcomeScenarioData> outcomeScenarioDataList = new List<OutcomeScenarioData>();

            outcomeScenarioDataList.Add(new OutcomeScenarioData("Done", new DataDescription(typeof(WireguardKeyPair), "Key Pair", false)));
            return outcomeScenarioDataList.ToArray();
        }
    }
    public ResultData Run(StepStartData data)
    {
        var secureRandom = new SecureRandom();
        
        // Generate private key using Curve25519
        var keyGenerationParameters = new KeyGenerationParameters(secureRandom, 255);
        var generator = new CipherKeyGenerator();
        generator.Init(keyGenerationParameters);
        var privateKeyBytes = generator.GenerateKey();
        
        // Convert private key to X25519 for WireGuard
        privateKeyBytes[0] &= 248;
        privateKeyBytes[31] &= 127;
        privateKeyBytes[31] |= 64;
        
        // Derive the public key from the private key
        var privateKey = new Org.BouncyCastle.Crypto.Parameters.Ed25519PrivateKeyParameters(privateKeyBytes, 0);
        var publicKeyBytes = privateKey.GeneratePublicKey().GetEncoded();
        
        // Convert to Base64
        var NewPrivateKey = Convert.ToBase64String(privateKeyBytes);
        var NewPublicKey = Convert.ToBase64String(publicKeyBytes);

        WireguardKeyPair KeyPair = new WireguardKeyPair(NewPublicKey, NewPrivateKey);
        
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary.Add("Key Pair", (object)KeyPair);
        return new ResultData("Done", (IDictionary<string, object>)dictionary);
    }
}