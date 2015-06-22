using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading.Tasks;

namespace ProjectCompton
{
    class DataProcessing
    {
        /* Class Description : This class describes functions that enables the processing,
          including noise removal, modification and filtering of the data downloaded from the 
          the tellmedave.cs.cornell.edu website. */

        String filterInstruction(String originalInstructions)
        {
            StringBuilder output = new StringBuilder();
            String modifiedInstruction = "";
            using (XmlReader reader = XmlReader.Create(new StringReader(originalInstructions)))
            {
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(output, ws))
                {

                    // Parse the file and display each of the nodes.
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                writer.WriteStartElement(reader.Name);
                                break;
                            case XmlNodeType.Text:
                                writer.WriteString(reader.Value);
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                writer.WriteProcessingInstruction(reader.Name, reader.Value);
                                break;
                            case XmlNodeType.Comment:
                                writer.WriteComment(reader.Value);
                                break;
                            case XmlNodeType.EndElement:
                                writer.WriteFullEndElement();
                                break;
                        }
                    }

                }
            }
            return modifiedInstruction;
        }

        void returnNormalDescription(String fileName)
        {
            /* Function Description: Takes the file which stores the entire data and outputs the 
             * filtered file. The format of the output and filtered file is same except few
             * style tags are removed. The main work done by this function is to call the filterInstruction
             * that takes the general instruction sequence in the original file and output a much
             * simplified instruction sequence by removing extra information, removing noise, modification etc. */

            StringBuilder output = new StringBuilder();
            String xmlString = System.IO.File.ReadAllText(Constants.rootPath+"/Data/"+fileName);

            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(output, ws))
                {
                    // Parse the file and display each of the nodes.
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                writer.WriteStartElement(reader.Name);
                                break;
                            case XmlNodeType.Text:
                                writer.WriteString(reader.Value);
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                writer.WriteProcessingInstruction(reader.Name, reader.Value);
                                break;
                            case XmlNodeType.Comment:
                                writer.WriteComment(reader.Value);
                                break;
                            case XmlNodeType.EndElement:
                                writer.WriteFullEndElement();
                                break;
                        }
                    }

                }
            }

        }
    }
}
