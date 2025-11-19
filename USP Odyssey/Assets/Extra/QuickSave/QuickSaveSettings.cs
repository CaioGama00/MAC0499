////////////////////////////////////////////////////////////////////////////////
//  
// @module Quick Save for Unity3D 
// @author Michael Clayton
// @support clayton.inds+support@gmail.com 
//
////////////////////////////////////////////////////////////////////////////////

namespace CI.QuickSave
{
    public class QuickSaveSettings
    {
        
        /// The type of encryption to use on the data
        
        public SecurityMode SecurityMode { get; set; }

        
        /// The type of compression to use on the data
        
        public CompressionMode CompressionMode { get; set; }

        
        /// If aes is selected as the security mode specify a password to use as the encryption key
        
        public string Password { get; set; }

        public QuickSaveSettings()
        {
            SecurityMode = SecurityMode.None;
            CompressionMode = CompressionMode.None;
        }
    }
}