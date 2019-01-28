using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using RolemapperService.WebApi.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RolemapperService.WebApi.Repositories
{
    public class KubernetesRepository : IKubernetesRepository
    {
        private readonly IKubernetes _client;
        private static readonly string ConfigMapName = "aws-auth";
        private static readonly string ConfigMapNamespace = "kube-system";

        public KubernetesRepository(IKubernetes client)
        {
            _client = client;
        }

        public async Task<string> GetAwsAuthConfigMap()
        {
            var awsAuthConfigMap = await GetConfigMap();
            var awsAuthConfigMapYaml = SerializeToYaml(awsAuthConfigMap);

            return awsAuthConfigMapYaml;
        }

        public async Task<string> GetAwsAuthConfigMapRoleMap()
        {
            var awsAuthConfigMap = await GetConfigMap();
            var awsAuthConfigMapMapRoles = awsAuthConfigMap.Data["mapRoles"];

            return awsAuthConfigMapMapRoles;
        }

        public async Task<string> ReplaceAwsAuthConfigMapRoleMap(string configMapRoleMap)
        {
            var awsAuthConfigMap = await GetConfigMap();
            
            awsAuthConfigMap.Data = new Dictionary<string, string>
            {
                { "mapRoles", configMapRoleMap }
            };

            var awsAuthConfigMapReplaced = await _client.ReplaceNamespacedConfigMapAsync(body: awsAuthConfigMap, name: ConfigMapName, namespaceParameter: ConfigMapNamespace);
            var awsAuthConfigMapYaml = SerializeToYaml(awsAuthConfigMapReplaced);

            return awsAuthConfigMapYaml;
        }

        public async Task<string> PatchAwsAuthConfigMapRoleMap(string configMapRoleMap)
        {
            var patch = new JsonPatchDocument<V1ConfigMap>();
            patch.Replace(c => c.Data["mapRoles"], configMapRoleMap);

            var configMapPatch = new V1Patch(patch);

            var awsAuthConfigMap = await _client.PatchNamespacedConfigMapAsync(body: configMapPatch, name: ConfigMapName, namespaceParameter: ConfigMapNamespace);
            var awsAuthConfigMapYaml = SerializeToYaml(awsAuthConfigMap);

            return awsAuthConfigMapYaml;
        }

        private async Task<V1ConfigMap> GetConfigMap()
        {
            return await _client.ReadNamespacedConfigMapAsync(name: ConfigMapName, namespaceParameter: ConfigMapNamespace, exact: true, export: true);
        }

        private KubernetesConfigMap GetCustomConfigMap(V1ConfigMap apiConfigMap)
        {
            return new KubernetesConfigMap
            {
                ApiVersion = apiConfigMap?.ApiVersion,
                Data = apiConfigMap?.Data,
                Kind = apiConfigMap?.Kind,
                Metadata = new Metadata
                {
                    Name = apiConfigMap?.Metadata?.Name,
                    NamespaceProperty = apiConfigMap?.Metadata?.NamespaceProperty
                }
            };
        }

        private string SerializeToYaml(V1ConfigMap apiConfigMap)
        {
            var customConfigMap = GetCustomConfigMap(apiConfigMap);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
                
            var yamlSerializedObject = serializer.Serialize(customConfigMap);

            return yamlSerializedObject;
        }
    }
}