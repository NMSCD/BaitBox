# How does this all work?

## Background
This extractor script acts a mod written for No Man's Sky, utilizing MonkeyMan192's [NMS.py](https://github.com/monkeyman192/NMS.py?ref=NMSCDBaitBox) to hook to the exe function responsible for generating bait descriptions, which are used as the basis for the data gathered by this project. Once triggered, the script stores the relevant memory pointers required to call the function itself, iterating through the item IDs defined in an input JSON file. Each generted description is then written to an output JSON file which may be modified as required.

## How To Use
* Download [NMS.py](https://github.com/monkeyman192/NMS.py?ref=NMSCDBaitBox), following it's setup instructions.
* Copy the contents of `ExtractGameData` to your selected mods folder for NMS.py, or set your NMS.py mods to it. Make sure that `mod.py` is **NOT** in the `Data` folder.
* Run the game using NMS.py, make sure all hooks are registered in the console and that the mod shows up in the pyMHF GUI.
* After loading into the game, open the Bait Box inventory *once*, in order to trigger the initial hook. This step is **necessary**, as the mod depends on the captured `cGcFishingData` memory pointer returned by this hook to call the function itself later on.
* Exit out of the Bait Box UI and press the `-` key on your keyboard. This will call the description generator function on each product ID defined in `Input.json`, and export each description (containing vital bait statistics) to `Output.json`. If the pyMHF console displays `Output Ready!`, everything's working properly!   