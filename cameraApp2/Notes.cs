using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraCOT
{
    public class Notes
    {
        List<string> listNotes = new List<string>(new string[] { "" });
   
        public int indexNoteShow { get; set; }

        public void InitListNote(List<string> notes)
        {
            listNotes = notes.ToList();
            indexNoteShow = 0;
        }

        public string NotesUp()
        {
            if (listNotes.Count - indexNoteShow > 0)
                indexNoteShow++;

            return listNotes[indexNoteShow];
        }

        public string NotesDown()
        {
            if(listNotes.Count > 0)
                indexNoteShow--;

            return listNotes[indexNoteShow];
        }

        public void NoteAdd(string str)
        {
            listNotes.Add(str);
        }

        public void NotesClear(string str)
        {
            listNotes.Add(str);
        }

        public void NoteRemove()
        {
            listNotes.RemoveAt(indexNoteShow);
            indexNoteShow--;
        }
    }
}
