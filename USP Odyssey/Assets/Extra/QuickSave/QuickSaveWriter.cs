////////////////////////////////////////////////////////////////////////////////
//  
// @module Quick Save for Unity3D 
// @author Michael Clayton
// @support clayton.inds+support@gmail.com 
//
////////////////////////////////////////////////////////////////////////////////

using CI.QuickSave.Core.Serialisers;

namespace CI.QuickSave
{
    public class QuickSaveWriter : QuickSaveBase
    {
        private QuickSaveWriter(string root, QuickSaveSettings settings)
        {
            _root = root;
            _settings = settings;
        }

        
        /// Creates a QuickSaveWriter on the specified root
        
        /// <param name="root">The root to write to</param>
        /// <returns>A QuickSaveWriter instance</returns>
        public static QuickSaveWriter Create(string root)
        {
            return Create(root, new QuickSaveSettings());
        }

        
        /// Creates a QuickSaveWriter on the specified root using the specified settings
        
        /// <param name="root">The root to write to</param>
        /// <param name="settings">Settings</param>
        /// <returns>A QuickSaveWriter instance</returns>
        public static QuickSaveWriter Create(string root, QuickSaveSettings settings)
        {
            QuickSaveWriter quickSaveWriter = new QuickSaveWriter(root, settings);
            quickSaveWriter.Load(true);
            return quickSaveWriter;
        }

        
        /// Writes an object to the specified key - you must called commit to write the data to file
        
        /// <typeparam name="T">The type of object to write</typeparam>
        /// <param name="key">The key this object will be saved under</param>
        /// <param name="value">The object to save</param>
        /// <returns>The QuickSaveWriter</returns>
        public QuickSaveWriter Write<T>(string key, T value)
        {
            if (Exists(key))
            {
                _items.Remove(key);
            }

            _items.Add(key, JsonSerialiser.SerialiseKey(value));

            return this;
        }

        
        /// Deletes the specified key if it exists
        
        /// <param name="key">The key to delete</param>
        public void Delete(string key)
        {
            if (Exists(key))
            {
                _items.Remove(key);
            }
        }

        
        /// Commits the changes to file
        
        public void Commit()
        {
            Save();
        }

        
        /// Attempts to commit the changes to file
        
        /// <returns>Was the commit successful</returns>
        public bool TryCommit()
        {
            try
            {
                Save();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}