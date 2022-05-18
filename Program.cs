using System.Diagnostics;
using System.Text.Json.Nodes;

class Program{
    static async Task Main()
    {
        // Get limit and offset variables from the user
        Console.WriteLine("Hello. Please provide a number of Pokemon to calculate.");
        
        string? input;
        // Continuously ask the user to provide a value to begin.
        do
        {
            input = Console.ReadLine();
            if(string.IsNullOrEmpty(input))
                Console.WriteLine("You didn't specify a value.\nPlease provide a number of pokemon to calculate.");
        } while (string.IsNullOrEmpty(input));

        int limit;
        // Continue to ask for input until we get something we can use.
        bool failedParse;
        do
        {
            failedParse = !int.TryParse(input, out limit);
            if(failedParse)
            {
                Console.WriteLine("Entry not recognized. Please enter a numerical value.\nPlease provide a number of pokemon to calculate.");
                input = Console.ReadLine();
            }
        } while (failedParse);
            
        // We now have a number of pokemon to retreive.
        // Get the amount of Pokemon to skip over.
        Console.WriteLine($"Number provided: {limit}");
        Console.WriteLine("Please provide a number of Pokemon to skip over.");

        // Retreive more input from the user like we just did.
        do
        {
            input = Console.ReadLine();
            if(string.IsNullOrEmpty(input))
                Console.WriteLine("You didn't specify a value.\nPlease provide a number of Pokemon to skip over.");
        } while (string.IsNullOrEmpty(input));
        
        // Reset the parse flag for another test
        failedParse = false;
        int offset;
        do
        {
            failedParse = !int.TryParse(input, out offset);
            if(failedParse)
            {
                Console.WriteLine("Entry not recognized. Please enter a numerical value.\nPlease provide a number of Pokemon to skip over.");
                input = Console.ReadLine();
            }
        } while (failedParse);
        Console.WriteLine($"Number provided: {offset}");

        Console.WriteLine("\n\nRetreiving specified number of Pokemon...");

        // Start a stopwatch to time the program
        Stopwatch watch = new Stopwatch();
        watch.Start();

        // Make an object to talk to the web
        HttpClient request = new HttpClient();

        // Send the GET request to the site
        HttpResponseMessage response = await request.GetAsync($"https://pokeapi.co/api/v2/pokemon/?limit={limit}&offset={offset}");

        // Parse the data that returned
        string responseJson = await response.Content.ReadAsStringAsync();
        JsonNode? pokemonArray = JsonNode.Parse(responseJson);

        // Make sure that we got some data
        if(pokemonArray == null)
        {
            Console.WriteLine("There was a problem retreiving pokemon information. Please try again.");
            return;
        }

        // Get to the items that we need
        JsonNode? results = pokemonArray["results"];

        // Make sure that we got the data we need to continue
        if(results == null)
        {
            Console.WriteLine("There was a problem retreiving pokemon information. Please try again.");
            return;
        }

        // Make sure to initialize a list to a specified value, it saves memory
        List<PokemonInfo> listOfPokemon = new List<PokemonInfo>(limit);

        // Fill out the names of our pokemon so we can get their info
        for(int i = 0; i < limit; i++)
        {
            listOfPokemon.Add(new PokemonInfo());
            listOfPokemon[i].name = results[i]?["name"]!.GetValue<string>();
        }

        // Retreive the heights and weights of the pokemon in the list at the same time
        await GetAggregateHeightAndWeights(listOfPokemon);


        // EXTRA CREDIT 
        // Split the heights and weights by types
        DictionaryWithCount typeWeights = CalculateWeightsByType(listOfPokemon);
        DictionaryWithCount typeHeights = CalculateHeightsByType(listOfPokemon);

        // Print results
        foreach(KeyValuePair<string, TypeTotal> kvp in typeWeights)
        {
            TypeTotal entry = typeWeights[kvp.Key];
            Console.WriteLine($"{Capitalize(kvp.Key)} average weight: {CalculateAverage(entry.value, entry.totalValuesAdded)}");
        }

        Console.WriteLine();

        foreach(KeyValuePair<string, TypeTotal> kvp in typeHeights)
        {
            TypeTotal entry = typeHeights[kvp.Key];
            Console.WriteLine($"{Capitalize(kvp.Key)} average height: {CalculateAverage(entry.value, entry.totalValuesAdded)}");
        }

        int totalHeight = 0;
        for(int i = 0; i < listOfPokemon.Count; i++)
        {
            totalHeight += listOfPokemon[i].height;
        }

        int totalWeight = 0;
        for(int i = 0; i < listOfPokemon.Count; i++)
        {
            totalWeight += listOfPokemon[i].weight;
        }

        Console.WriteLine($"\n\nTotal average height of these Pokemon is: {CalculateAverage(totalHeight, limit)}.\nTotal average weight of these Pokemon is: {CalculateAverage(totalWeight, limit)}.");

        // Stop the clock and report.
        watch.Stop();
        Console.WriteLine($"Response took {watch.Elapsed}");
    }

    public static float CalculateAverage(float total, int divisor)
    {
        return total / divisor;
    }

    public static DictionaryWithCount CalculateWeightsByType(List<PokemonInfo> pokemonList) 
    {
        DictionaryWithCount weightsByType = new DictionaryWithCount();

        for(int i = 0; i < pokemonList.Count; i++)
        {
            for(int j = 0; j < pokemonList[i].types.Count; j++)
            {
                // We don't have this type yet, add it to the dictionary
                if(!weightsByType.ContainsKey(pokemonList[i].types[j]))
                {
                    TypeTotal typeTotal = new TypeTotal();
                    typeTotal.value = pokemonList[i].weight;
                    typeTotal.totalValuesAdded = 1;
                    weightsByType.Add(pokemonList[i].types[j], typeTotal);
                    continue;
                }
                // We already have at an entry with this type
                TypeTotal newValue = weightsByType[pokemonList[i].types[j]];

                // Update the value with the weight of this entry
                newValue.value += pokemonList[i].weight;
                newValue.totalValuesAdded++;
                weightsByType[pokemonList[i].types[j]] = newValue;
            }            
        }
        
        return weightsByType;
    }

    public static DictionaryWithCount CalculateHeightsByType(List<PokemonInfo> pokemonList) 
    {
        DictionaryWithCount heightsByType = new DictionaryWithCount();

        for(int i = 0; i < pokemonList.Count; i++)
        {
            for(int j = 0; j < pokemonList[i].types?.Count; j++)
            {
                // We don't have this type yet, add it to the dictionary
                if(!heightsByType.ContainsKey(pokemonList[i].types[j]))
                {
                    TypeTotal typeTotal = new TypeTotal();
                    typeTotal.value = pokemonList[i].height;
                    typeTotal.totalValuesAdded = 1;
                    heightsByType.Add(pokemonList[i].types[j], typeTotal);
                    continue;
                }
                // We already have at an entry with this type
                TypeTotal newValue = heightsByType[pokemonList[i].types[j]];

                // Update the value with the height of this entry
                newValue.value += pokemonList[i].height;
                newValue.totalValuesAdded++;
                heightsByType[pokemonList[i].types[j]] = newValue;
            }            
        }
        
        return heightsByType;
    }

    public static async Task GetAggregateHeightAndWeights(List<PokemonInfo> pokemonList)
    {
        HttpClient request = new HttpClient();

        //Start with a list of URLs
        List<string> urls = new List<string>(pokemonList.Count);

        for(int i = 0; i < pokemonList.Count; i++)
        {
            urls.Add($"https://pokeapi.co/api/v2/pokemon/{pokemonList[i].name}/");
        }        

        //Start requests for all of them
        List<Task<HttpResponseMessage>> requests  = urls.Select(url => request.GetAsync(url)).ToList();

        //Wait for all the requests to finish
        await Task.WhenAll(requests);

        //Get the responses
        List<HttpResponseMessage> responses = requests.Select(task => task.Result).ToList();

        for(int i = 0; i < responses.Count; i++)
        {
            // Extract the response
            string responseJson = await responses[i].Content.ReadAsStringAsync();
            
            // Parse the data
            JsonNode? node = JsonNode.Parse(responseJson);

            // Make sure there's something to use
            if(node == null)
            {
                Console.WriteLine($"There was a problem when retreving information");
                return;
            }
            
            // Store basic information about the pokemon
            pokemonList[i].name = node["name"]!.GetValue<string>();
            pokemonList[i].height = node["height"]!.GetValue<int>();
            pokemonList[i].weight = node["weight"]!.GetValue<int>();

            // Store all the types this pokemon is
            List<JsonNode?> listOfTypes = node["types"]!.AsArray().ToList();
            for(int j = 0; j < listOfTypes.Count; j++)
            {
                pokemonList[i].types.Add(listOfTypes[j]!["type"]!["name"]!.GetValue<string>());
            }

            // Console.WriteLine($"Name: {pokemonList[i].name}");
            // Console.WriteLine($"Height: {pokemonList[i].height}");
            // Console.WriteLine($"Weight: {pokemonList[i].weight}");

        }
    }

    public static string Capitalize(string word)
    {
        // Make sure there is something in the args
        if (string.IsNullOrEmpty(word))
            return string.Empty;

        // Split the word up
        char[] letters = word.ToCharArray();

        // Set first letter to capital
        letters[0] = char.ToUpper(letters[0]);
        return new string(letters);
    }
}

public class PokemonInfo{
    public string? name;
    public int height;
    public int weight;

    public List<string> types = new List<string>();
}

public struct TypeTotal{
    public float value;
    public int totalValuesAdded;
}

public class DictionaryWithCount : Dictionary<string, TypeTotal>
{
    public void Add(string type, float value, int count)
    {
        TypeTotal info;
        info.value = value;
        info.totalValuesAdded = count;
        this.Add(type, info);
    }
}