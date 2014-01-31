using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("ShipCollection")]
public class ShipContainer {
	[XmlArray("Ships")]
	[XmlArrayItem("Ship")]
	public List<ShipScript> ships = new List<ShipScript> ();

	public ShipContainer() {
		ships.Add (new ShipScript ());
	}


	public void Save(string path) {
		var serializer = new XmlSerializer(typeof(ShipContainer));
		using(var stream = new FileStream(path, FileMode.Create))
		{
			serializer.Serialize(stream, this);
		}
	}

	public static ShipContainer Load(string path) {
		var serializer = new XmlSerializer(typeof(ShipContainer));
		using(var stream = new FileStream(path, FileMode.Open))
		{
			return serializer.Deserialize(stream) as ShipContainer;
		}
	}
}