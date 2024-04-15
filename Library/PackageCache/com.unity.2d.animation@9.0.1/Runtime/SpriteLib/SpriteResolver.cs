using System;
using UnityEngine.Animations;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.U2D.Common;

namespace UnityEngine.U2D.Animation
{
    /// <summary>
    /// Updates a SpriteRenderer's Sprite reference on the Category and Label value it is set.
    /// </summary>
    /// <Description>
    /// By setting the SpriteResolver's Category and Label value, it will request for a Sprite from
    /// a SpriteLibrary Component the Sprite that is registered for the Category and Label.
    /// If a SpriteRenderer is present in the same GameObject, the SpriteResolver will update the
    /// SpriteRenderer's Sprite reference to the corresponding Sprite.
    /// </Description>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("2D Animation/Sprite Resolver")]
    [IconAttribute(IconUtility.IconPath + "Animation.SpriteResolver.png")]
    [DefaultExecutionOrder(-2)]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.animation@latest/index.html?subfolder=/manual/SLAsset.html%23sprite-resolver-component")]
    [MovedFrom("UnityEngine.Experimental.U2D.Animation")]
    public class SpriteResolver : MonoBehaviour, ISerializationCallbackReceiver, IPreviewable
    {
        // SpriteHash is the new animation key.
        // We are keeping the old ones so that the animation clip doesn't break
        
        // These are for animation
        [SerializeField]
        float m_CategoryHash = 0;
        [SerializeField]
        float m_labelHash = 0;

        [SerializeField]
        float m_SpriteKey = 0;

        [SerializeField, DiscreteEvaluation]
        int m_SpriteHash = 0;
        
        // For comparing hash values
        int m_CategoryHashInt;
        int m_LabelHashInt;

        // For OnUpdate during animation playback
        int m_PreviousCategoryHash;
        int m_PreviousLabelHash;
        int m_PreviousSpriteKeyInt;
        int m_PreviousSpriteHash;

#if UNITY_EDITOR
        bool m_SpriteLibChanged;
        
        /// <summary>
        /// Raised when object is deserialized in the Editor.
        /// </summary>
        public event Action onDeserializedCallback = () => { };
#endif

        void Reset()
        {
            // If the Sprite referred to by the SpriteRenderer exist in the library,
            // we select the Sprite
            if(spriteRenderer)
                SetSprite(spriteRenderer.sprite);
        }

        void SetSprite(Sprite sprite)
        {
            var sl = spriteLibrary;
            if (sl != null && sprite != null)
            {
                foreach (var cat in sl.categoryNames)
                {
                    var entries = sl.GetEntryNames(cat);
                    foreach (var ent in entries)
                    {
                        if (sl.GetSprite(cat, ent) == sprite)
                        {
                            m_SpriteHash = SpriteLibrary.GetHashForCategoryAndEntry(cat, ent);
                            return;
                        }
                    }
                }
            }
        }
        
        void OnEnable()
        {
            InitializeSerializedData();
            ResolveSpriteToSpriteRenderer();
        }

        void InitializeSerializedData()
        {
            m_CategoryHashInt = SpriteLibraryUtility.Convert32BitTo30BitHash(InternalEngineBridge.ConvertFloatToInt(m_CategoryHash));
            m_PreviousCategoryHash = m_CategoryHashInt;
            m_LabelHashInt = SpriteLibraryUtility.Convert32BitTo30BitHash(InternalEngineBridge.ConvertFloatToInt(m_labelHash));
            m_PreviousLabelHash = m_LabelHashInt;
            m_PreviousSpriteKeyInt = SpriteLibraryUtility.Convert32BitTo30BitHash(InternalEngineBridge.ConvertFloatToInt(m_SpriteKey));
            m_SpriteKey = InternalEngineBridge.ConvertIntToFloat(m_PreviousSpriteKeyInt);
            
            if (m_SpriteHash == 0)
            {
                if (m_SpriteKey != 0f)
                    m_SpriteHash = InternalEngineBridge.ConvertFloatToInt(m_SpriteKey);
                else
                    m_SpriteHash = ConvertCategoryLabelHashToSpriteKey(spriteLibrary, m_CategoryHashInt, m_LabelHashInt);
            }

            m_PreviousSpriteHash = m_SpriteHash;            
        }

        SpriteRenderer spriteRenderer => GetComponent<SpriteRenderer>();

        /// <summary>
        /// Set the Category and label to use.
        /// </summary>
        /// <param name="category">Category to use.</param>
        /// <param name="label">Label to use.</param>
        /// <returns>True if the Category and Label were successfully set.</returns>
        public bool SetCategoryAndLabel(string category, string label)
        {
            m_SpriteHash = SpriteLibrary.GetHashForCategoryAndEntry(category, label);
            m_PreviousSpriteHash = m_SpriteHash;
            return ResolveSpriteToSpriteRenderer();
        }

        /// <summary>
        /// Get the Category set for the SpriteResolver.
        /// </summary>
        /// <returns>The Category's name.</returns>
        public string GetCategory()
        {
            var returnString = "";
            var sl = spriteLibrary;
            if (sl)
            {
                sl.GetCategoryAndEntryNameFromHash(m_SpriteHash, out returnString, out _);
            }

            return returnString;
        }

        /// <summary>
        /// Get the Label set for the SpriteResolver.
        /// </summary>
        /// <returns>The Label's name.</returns>
        public string GetLabel()
        {
            var returnString = "";
            var sl = spriteLibrary;
            if (sl)
                sl.GetCategoryAndEntryNameFromHash(m_SpriteHash, out _, out returnString);

            return returnString;
        }

        /// <summary>
        /// Property to get the SpriteLibrary the SpriteResolver is resolving from.
        /// </summary>
        public SpriteLibrary spriteLibrary => gameObject.GetComponentInParent<SpriteLibrary>(true);
        
        /// <summary>
        /// Empty method. Implemented for the IPreviewable interface.
        /// </summary>
        public void OnPreviewUpdate() { }
        
#if UNITY_EDITOR         
        void OnDidApplyAnimationProperties()
        {
            if(IsInGUIUpdateLoop())
                ResolveUpdatedValue();
        }        
#endif        
        
        static bool IsInGUIUpdateLoop() => Event.current != null;
        
        void LateUpdate()
        {
            ResolveUpdatedValue();
        }

        void ResolveUpdatedValue()
        {
            if (m_SpriteHash != m_PreviousSpriteHash)
            {
                m_PreviousSpriteHash = m_SpriteHash;
                ResolveSpriteToSpriteRenderer();
            }
            else
            {
                var spriteKeyInt = InternalEngineBridge.ConvertFloatToInt(m_SpriteKey);
                if (spriteKeyInt != m_PreviousSpriteKeyInt)
                {
                    m_SpriteHash = SpriteLibraryUtility.Convert32BitTo30BitHash(spriteKeyInt);
                    m_PreviousSpriteKeyInt = spriteKeyInt;
                    ResolveSpriteToSpriteRenderer();               
                }
                else
                {
                    m_CategoryHashInt = SpriteLibraryUtility.Convert32BitTo30BitHash(InternalEngineBridge.ConvertFloatToInt(m_CategoryHash));
                    m_LabelHashInt = SpriteLibraryUtility.Convert32BitTo30BitHash(InternalEngineBridge.ConvertFloatToInt(m_labelHash));
                    if (m_LabelHashInt != m_PreviousLabelHash || m_CategoryHashInt != m_PreviousCategoryHash)
                    {
                        if (spriteLibrary != null)
                        {
                            m_PreviousCategoryHash = m_CategoryHashInt;
                            m_PreviousLabelHash = m_LabelHashInt;
                            m_SpriteHash = ConvertCategoryLabelHashToSpriteKey(spriteLibrary, m_CategoryHashInt, m_LabelHashInt);
                            m_PreviousSpriteHash = m_SpriteHash;
                            ResolveSpriteToSpriteRenderer();
                        }
                    }                    
                }
            }
        }

        internal static int ConvertCategoryLabelHashToSpriteKey(SpriteLibrary library, int categoryHash, int labelHash)
        {
            if (library != null)
            {
                foreach(var category in library.categoryNames)
                {
                    if (categoryHash == SpriteLibraryUtility.GetStringHash(category))
                    {
                        var entries = library.GetEntryNames(category);
                        if (entries != null)
                        {
                            foreach (var entry in entries)
                            {
                                if (labelHash == SpriteLibraryUtility.GetStringHash(entry))
                                {
                                    return SpriteLibrary.GetHashForCategoryAndEntry(category, entry);
                                }
                            }
                        }
                    }
                }
            }

            return 0;
        }
        
        internal Sprite GetSprite(out bool validEntry)
        {
            var lib = spriteLibrary;
            if (lib != null)
            {
                return lib.GetSpriteFromCategoryAndEntryHash(m_SpriteHash, out validEntry);
            }
            validEntry = false;
            return null;
        }

        /// <summary>
        /// Set the Sprite in SpriteResolver to the SpriteRenderer component that is in the same GameObject.
        /// </summary>
        /// <returns>True if it successfully resolved the Sprite.</returns>
        public bool ResolveSpriteToSpriteRenderer()
        {
            m_PreviousSpriteHash = m_SpriteHash;
            var sprite = GetSprite(out var validEntry);
            var sr = spriteRenderer;
            if (sr != null && (sprite != null || validEntry))
                sr.sprite = sprite;
            return validEntry;
        }
        
        void OnTransformParentChanged()
        {
            ResolveSpriteToSpriteRenderer();
#if UNITY_EDITOR
            spriteLibChanged = true;
#endif
        }

#if UNITY_EDITOR
        internal bool spriteLibChanged
        {
            get => m_SpriteLibChanged;
            set => m_SpriteLibChanged = value;
        }
#endif
        
        /// <summary>
        /// Called before object is serialized.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        /// <summary>
        /// Called after object is deserialized.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if UNITY_EDITOR
            onDeserializedCallback();
#endif            
        }
    }
}
