// Territory.cs
//

using System;
using System.Collections;
using System.Html;
using System.Runtime.CompilerServices;
using jQueryApi;
using jQueryApi.Templating;

namespace Northwind.Client.Models
{
    public class Territory : EntityWithAssignedId
    {
        [PreserveCase] public string Description;

        [PreserveCase] public Region RegionBelongingTo;
    }

    public interface ITerritoryService
    {
        void TerritorySuggestions(jQueryEvent eventHandler);

        void SetViewModel(EmployeeViewModel viewModel);

        string TerritoriestoCSV(Territory[] territories);
    }

    public sealed class TerritoryService : ITerritoryService
    {
        public EmployeeViewModel EmployeeViewModel;

        public TerritoryService()
        {
            RegisterClickHandlers();
        }

        private void RegisterClickHandlers() 
        {
            jQuery.Select("#TerritoriesAutoSuggest").Live("keyup", this.TerritorySuggestions);

            jQuery.Document.Click(delegate(jQueryEvent e)
            {
                if (e.CurrentTarget.ID != "suggestions")
                {
                    jQuery.Select("#suggestions").Children().Remove();
                }
            });

            jQuery.Select(".child-suggestion").Live("click", this.AddTerritoryToInput);
        }

        public void SetViewModel(EmployeeViewModel viewModel) 
        {
            this.EmployeeViewModel = viewModel;
        }

        public void TerritorySuggestions(jQueryEvent eventHandler) 
        {
            eventHandler.PreventDefault();
            jQuery.Select("#suggestions").Empty();
            jQuery.Select("#suggestions").Show();
            InputElement territoryInput = (InputElement)eventHandler.CurrentTarget;

            string[] territoryArray = territoryInput.Value.Split(',');

            int counter = 0;
            for (int index = 0; index < this.EmployeeViewModel.AvailableTerritories.Length; index++) 
            {
                // Only show the first 3 results in the UI
                if (counter > 2) 
                {
                    break;
                }

                Territory availableTerritory = this.EmployeeViewModel.AvailableTerritories[index];
                RegularExpression r = new RegularExpression(territoryArray[territoryArray.Length - 1].Trim(), "i");

                if (availableTerritory.Description.Search(r) != -1) 
                {
                    jQuery.Select("#suggestions").Append(
                        jQuery.FromHtml("<div style=\"margin-left:7px;\" class=\"child-suggestion\">" + availableTerritory.Description + "</div>"));

                    counter = counter + 1;
                }
            }
        }

        public string TerritoriestoCSV(Territory[] territories)
        {
            string territoriesCommaString = new string();

            for (int index = 0; index < territories.Length; index++)
            {
                Territory territory = territories[index];
                if (index == territories.Length - 1)
                {
                    territoriesCommaString = territoriesCommaString + territory.Description.Trim();
                }
                else
                {
                    territoriesCommaString = territoriesCommaString + territory.Description.Trim() + ", ";
                }
            }

            return territoriesCommaString;
        }

        public void AddTerritoryToInput(jQueryEvent eventHandler) 
        {
            jQueryObject autoSuggestBox = jQuery.Select("#TerritoriesAutoSuggest");

            string[] territoryArray = autoSuggestBox.GetValue().Split(',');

            territoryArray[territoryArray.Length - 1] = " " + eventHandler.CurrentTarget.InnerHTML.Trim() + ", ";

            jQuery.Select("#TerritoriesAutoSuggest").Value(territoryArray.ToString());
        }
    }
}
