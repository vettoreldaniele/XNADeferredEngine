using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.ComponentModel;
using System.IO;



namespace DeferredPipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "Deferred Renderer Model")]
    public class DeferredRendererModel : ModelProcessor
    {
        // Always generate tangent and binormal data.
        [Browsable(false)]
        public override bool GenerateTangentFrames
        {
            get { return true; }
            set { }
        }

        // Store only UV coords, normals, binormals and tangents.
        static IList<string> acceptableVertexChannelNames =
         new string[]
        {
         VertexChannelNames.TextureCoordinate(0),
         VertexChannelNames.Normal(0),
         VertexChannelNames.Binormal(0),
         VertexChannelNames.Tangent(0),
        };

        // Directory of the model.
        String directory;

        // Normal and specular map textures and keys.
        private string normalMapTexture;
        private string normalMapKey = "NormalMap";
        private string specularMapTexture;
        private string specularMapKey = "SpecularMap";

        [DisplayName("Normal Map Texture")]
        [Description("If set, this file will be used as the normal map on the model, " +
        "overriding anything found in the opaque data.")]
        [DefaultValue("")]
        public string NormalMapTexture
        {
            get { return normalMapTexture; }
            set { normalMapTexture = value; }
        }

        [DisplayName("Normal Map Key")]
        [Description("This is the key that will be used to search the normal map in the opaque data of the model")]
        [DefaultValue("NormalMap")]
        public string NormalMapKey
        {
            get { return normalMapKey; }
            set { normalMapKey = value; }
        }

        [DisplayName("Specular Map Texture")]
        [Description("If set, this file will be used as the specular map on the model, " +
        "overriding anything found in the opaque data.")]
        [DefaultValue("")]
        public string SpecularMapTexture
        {
            get { return specularMapTexture; }
            set { specularMapTexture = value; }
        }

        [DisplayName("Specular Map Key")]
        [Description("This is the key that will be used to search the specular map in the opaque data of the model")]
        [DefaultValue("SpecularMap")]
        public string SpecularMapKey
        {
            get { return specularMapKey; }
            set { specularMapKey = value; }
        }

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            // Get directory.
            directory = Path.GetDirectoryName(input.Identity.SourceFilename);

            LookUpTextures(input);

            ModelContent modelContent = base.Process(input, context);

            // Specular and normal maps.
            ExternalReference<TextureContent> extSpecTexture = modelContent.Meshes[0].MeshParts[0].Material.Textures["SpecularMap"];
            ExternalReference<TextureContent> extNormTexture = modelContent.Meshes[0].MeshParts[0].Material.Textures["NormalMap"];

            OpaqueDataDictionary processorParams = new OpaqueDataDictionary();
            processorParams.Add("GenerateMipmaps", true);
            processorParams.Add("PremultiplyAlpha", true);
            processorParams.Add("TextureFormat", TextureProcessorOutputFormat.DxtCompressed);

            ExternalReference<TextureContent> textureSpec = context.BuildAsset<TextureContent, TextureContent>(
                       extSpecTexture,
                        "TextureProcessor",processorParams,null,null
                       );
            ExternalReference<TextureContent> textureNorm = context.BuildAsset<TextureContent, TextureContent>(
                       extNormTexture,
                         "TextureProcessor",processorParams,null,null
                       );

            var texturesTag = new ExternalReference<TextureContent>[2];
            texturesTag[0] = textureNorm;
            texturesTag[1] = textureSpec;

            modelContent.Tag = texturesTag; 

            return modelContent;


        }

    /*    protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                                                 ContentProcessorContext context)
        {
            EffectMaterialContent deferredShadingMaterial = new EffectMaterialContent();
            deferredShadingMaterial.Effect = new ExternalReference<EffectContent>("Effects/GBufferEffect.fx");

            foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture
            in material.Textures)
            {
                if ((texture.Key == "Texture") ||
                        (texture.Key == "NormalMap") ||
                        (texture.Key == "SpecularMap"))
                    deferredShadingMaterial.Textures.Add(texture.Key, texture.Value);
            }
            return context.Convert<MaterialContent, MaterialContent>(deferredShadingMaterial, typeof(MaterialProcessor).Name);
        } */



        private void LookUpTextures(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Paths to the normal and specular map texture.
                string normalMapPath;
                string specularMapPath;

                // If the SpecularMapTexture property is set, we use that normal map for all meshes in the model.
                // This overrides anything else.
                if (!String.IsNullOrEmpty(SpecularMapTexture))
                {
                    specularMapPath = SpecularMapTexture;
                }
                else
                {
                    // If SpecularMapTexture is not set, we look into the opaque data of the model, and search for a texture with the key equal to SpecularMapKey.
                    specularMapPath = mesh.OpaqueData.GetValue<string>(specularMapKey, null);
                }

                // If the SpecularMapTexture Property was not used, and the key was not found in the model, than specularMapPath would have the value null.
                if (specularMapPath == null)
                {
                    // If a key with the required name is not found, we make a final attempt and search, in the same directory as the model,
                    // for a texture named meshname_s.tga, where meshname is the name of a mesh inside the model.
                    specularMapPath = Path.Combine(directory, mesh.Name + "_s.tga");

                    if (!File.Exists(specularMapPath))
                    {
                        // If this also fails (that texture does not exist), we use a default texture, named null_specular.tga.
                        specularMapPath = "null_specular.tga";
                    }
                }
                else
                {
                    specularMapPath = Path.Combine(directory, specularMapPath);
                }


                // If the NormalMapTexture property is set, we use that normal map for all meshes in the model.
                // This overrides anything else.
                if (!String.IsNullOrEmpty(NormalMapTexture))
                {
                    normalMapPath = NormalMapTexture;
                }
                else
                {
                    // If NormalMapTexture is not set, we look into the opaque data of the model, and search for a texture with the key equal to NormalMapKey.
                    normalMapPath = mesh.OpaqueData.GetValue<string>(NormalMapKey, null);
                }

                // If the NormalMapTexture Property was not used, and the key was not found in the model, than normalMapPath would have the value null.
                if (normalMapPath == null)
                {
                    // If a key with the required name is not found, we make a final attempt and search, in the same directory as the model,
                    // for a texture named meshname_n.tga, where meshname is the name of a mesh inside the model.
                    normalMapPath = Path.Combine(directory, mesh.Name + "_n.tga");

                    if (!File.Exists(normalMapPath))
                    {
                        // If this also fails (that texture does not exist), we use a default texture, named null_normal.tga.
                        normalMapPath = "null_normal.tga";
                    }
                }
                else
                {
                    normalMapPath = Path.Combine(directory, normalMapPath);
                }

                // From this point forward, we will name the textures "NormalMap" and "SpecularMap". This is what our shaders will expect.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    // In some .fbx files, the key might be found in the textures collection, but not in the mesh, as we checked above. If this is the case, we need to get it out, and
                    // add it with the "NormalMap" key.
                    if (geometry.Material.Textures.ContainsKey(normalMapKey))
                    {
                        ExternalReference<TextureContent> texRef = geometry.Material.Textures[normalMapKey];
                        geometry.Material.Textures.Remove(normalMapKey);
                        geometry.Material.Textures.Add("NormalMap", texRef);
                    }
                    else
                        geometry.Material.Textures.Add("NormalMap",
                                                            new ExternalReference<TextureContent>(normalMapPath));
                   
                    if (geometry.Material.Textures.ContainsKey(specularMapKey))
                    {
                        ExternalReference<TextureContent> texRef = geometry.Material.Textures[specularMapKey];
                        geometry.Material.Textures.Remove(specularMapKey);
                        geometry.Material.Textures.Add("SpecularMap", texRef);
                    }
                    else
                        geometry.Material.Textures.Add("SpecularMap",
                                    new ExternalReference<TextureContent>(specularMapPath));

                }

            }

            // Go through all children and apply LookUpTextures recursively
            foreach (NodeContent child in node.Children)
            {
                LookUpTextures(child);
            }
        }


        protected override void ProcessVertexChannel(GeometryContent geometry, int vertexChannelIndex, ContentProcessorContext context)
        {
            String vertexChannelName = geometry.Vertices.Channels[vertexChannelIndex].Name;

            // If this vertex channel has an acceptable name, process it as normal.
            if (acceptableVertexChannelNames.Contains(vertexChannelName))
            {
                base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
            }
            else // Otherwise, remove it from the vertex channels: it's extra data we don't need.
            {
                geometry.Vertices.Channels.Remove(vertexChannelName);
            }

        }
    }
}
