//! Northwind.Client.debug.js
//

(function() {
function executeScript() {

Type.registerNamespace('Northwind.Client');

////////////////////////////////////////////////////////////////////////////////
// Northwind.Client.EmployeeModule

Northwind.Client.EmployeeModule = function Northwind_Client_EmployeeModule() {
    /// <field name="_employeeViewModel" type="Northwind.Client.Models.EmployeeViewModel">
    /// </field>
    /// <field name="_selectedEmployee" type="Northwind.Client.Models.Employee">
    /// </field>
    /// <field name="_employeeService" type="Northwind.Client.Models.IEmployeeService">
    /// </field>
    /// <field name="_territoryService" type="Northwind.Client.Models.ITerritoryService">
    /// </field>
    this._employeeService = new Northwind.Client.Models.EmployeeService();
    this._territoryService = new Northwind.Client.Models.TerritoryService();
    this._initialize();
    this._registerClickHandlers();
}
Northwind.Client.EmployeeModule.prototype = {
    _employeeViewModel: null,
    _selectedEmployee: null,
    _employeeService: null,
    _territoryService: null,
    
    get_selectedEmployee: function Northwind_Client_EmployeeModule$get_selectedEmployee() {
        /// <value type="Northwind.Client.Models.Employee"></value>
        return this._selectedEmployee;
    },
    set_selectedEmployee: function Northwind_Client_EmployeeModule$set_selectedEmployee(value) {
        /// <value type="Northwind.Client.Models.Employee"></value>
        this._selectedEmployee = value;
        return value;
    },
    
    get_employeeViewModel: function Northwind_Client_EmployeeModule$get_employeeViewModel() {
        /// <value type="Northwind.Client.Models.EmployeeViewModel"></value>
        return this._employeeViewModel;
    },
    set_employeeViewModel: function Northwind_Client_EmployeeModule$set_employeeViewModel(value) {
        /// <value type="Northwind.Client.Models.EmployeeViewModel"></value>
        this._employeeViewModel = value;
        this._territoryService.setViewModel(value);
        this._renderTable();
        return value;
    },
    
    _registerClickHandlers: function Northwind_Client_EmployeeModule$_registerClickHandlers() {
        $('#create-employee').live('click', ss.Delegate.create(this, this._create));
        $('#employee-save').live('click', ss.Delegate.create(this, this._create));
        $('#edit').live('click', ss.Delegate.create(this, this._edit));
        $('#delete').live('click', ss.Delegate.create(this, this._remove));
        $('#details').live('click', ss.Delegate.create(this, this._details));
        $('#TerritoriesAutoSuggest').live('keyup', ss.Delegate.create(this._territoryService, this._territoryService.territorySuggestions));
        $(document).click(ss.Delegate.create(this, function(e) {
            $('#suggestions').hide();
        }));
    },
    
    _initialize: function Northwind_Client_EmployeeModule$_initialize() {
        $('#loading').show();
        $.getJSON('EmployeeRia/GetEmployeeFormViewModel', ss.Delegate.create(this, function(response) {
            this.set_employeeViewModel(response);
        }));
    },
    
    _create: function Northwind_Client_EmployeeModule$_create(eventHandler) {
        /// <param name="eventHandler" type="jQueryEvent">
        /// </param>
        eventHandler.preventDefault();
        if (eventHandler.currentTarget.id === 'create-employee') {
            var employee = new Northwind.Client.Models.Employee();
            this._employeeService.displayForm(employee);
            return;
        }
        $('.error-message').remove();
        $('#employee-save').attr('disabled', 'true');
        $.post('EmployeeRia/Edit', $('#employee-form-create').serialize(), ss.Delegate.create(this, function(response) {
            var employee = response;
            if (employee.Valid) {
                $('#main').empty();
                $('#employee-form').remove();
                this._initialize();
                return;
            }
            this._employeeService.showValidationResults(employee);
        }));
    },
    
    _edit: function Northwind_Client_EmployeeModule$_edit(eventHandler) {
        /// <param name="eventHandler" type="jQueryEvent">
        /// </param>
        this._getCurrentEmployeeById(eventHandler.currentTarget.getAttribute('employeeId').toString());
        this._employeeService.displayForm(this.get_selectedEmployee());
    },
    
    _remove: function Northwind_Client_EmployeeModule$_remove(eventHandler) {
        /// <param name="eventHandler" type="jQueryEvent">
        /// </param>
        this._getCurrentEmployeeById(eventHandler.currentTarget.getAttribute('employeeId').toString());
        $.post('EmployeeRia/Delete/' + this.get_selectedEmployee().Id, ss.Delegate.create(this, function(response) {
            this.get_employeeViewModel().Employees = response;
            this._renderTable();
        }));
    },
    
    _details: function Northwind_Client_EmployeeModule$_details(eventHandler) {
        /// <param name="eventHandler" type="jQueryEvent">
        /// </param>
        this._getCurrentEmployeeById(eventHandler.currentTarget.getAttribute('employeeId').toString());
        var employeeDetails = $.tmpl($('#employee-detail-tmpl').html(), this.get_selectedEmployee());
        $('#main').append(employeeDetails);
        $('#employee-detail').dialog({minWidth:500,close: function(event, ui){$(this).dialog('destroy').remove();}});
    },
    
    _getCurrentEmployeeById: function Northwind_Client_EmployeeModule$_getCurrentEmployeeById(id) {
        /// <param name="id" type="String">
        /// </param>
        var $enum1 = ss.IEnumerator.getEnumerator(this._employeeViewModel.Employees);
        while ($enum1.moveNext()) {
            var employee = $enum1.get_current();
            if (employee.Id.toString() === id) {
                employee.TerritoriesString = this._territoryService.territoriestoCSV(employee.Territories);
                this.set_selectedEmployee(employee);
                break;
            }
        }
    },
    
    _renderTable: function Northwind_Client_EmployeeModule$_renderTable() {
        if ((this.get_employeeViewModel().Employees.length === 0 & this.get_employeeViewModel().AvailableTerritories.length === 0) === 1) {
            return;
        }
        $('#main').empty();
        this._attachTemplates();
        $('#loading').hide();
    },
    
    _attachTemplates: function Northwind_Client_EmployeeModule$_attachTemplates() {
        var slug = new Boolean();
        $('#main').empty();
        var employeeTable = $.tmpl($('#employees-table-tmpl').html(), slug);
        $('#main').append(employeeTable);
        var $enum1 = ss.IEnumerator.getEnumerator(this.get_employeeViewModel().Employees);
        while ($enum1.moveNext()) {
            var employee = $enum1.get_current();
            var employeeRow = $.tmpl($('#employees-table-row-tmpl').html(), employee);
            $('#employees-table tbody').append(employeeRow);
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// Northwind.Client.EmployeeRia

Northwind.Client.EmployeeRia = function Northwind_Client_EmployeeRia() {
}
Northwind.Client.EmployeeRia.main = function Northwind_Client_EmployeeRia$main() {
    var employeeModule = new Northwind.Client.EmployeeModule();
}


Type.registerNamespace('Northwind.Client.Models');

////////////////////////////////////////////////////////////////////////////////
// Northwind.Client.Models.IEmployeeService

Northwind.Client.Models.IEmployeeService = function() { 
};
Northwind.Client.Models.IEmployeeService.prototype = {
    displayForm : null,
    showValidationResults : null
}
Northwind.Client.Models.IEmployeeService.registerInterface('Northwind.Client.Models.IEmployeeService');


////////////////////////////////////////////////////////////////////////////////
// Northwind.Client.Models.ITerritoryService

Northwind.Client.Models.ITerritoryService = function() { 
};
Northwind.Client.Models.ITerritoryService.prototype = {
    territorySuggestions : null,
    setViewModel : null,
    territoriestoCSV : null
}
Northwind.Client.Models.ITerritoryService.registerInterface('Northwind.Client.Models.ITerritoryService');


////////////////////////////////////////////////////////////////////////////////
// Northwind.Client.Models.Employee

Northwind.Client.Models.Employee = function Northwind_Client_Models_Employee() {
    /// <field name="FirstName" type="String">
    /// </field>
    /// <field name="FullName" type="String">
    /// </field>
    /// <field name="LastName" type="String">
    /// </field>
    /// <field name="PhoneExtension" type="Number" integer="true">
    /// </field>
    /// <field name="Territories" type="Array" elementType="Territory">
    /// </field>
    /// <field name="TerritoriesString" type="String">
    /// </field>
    Northwind.Client.Models.Employee.initializeBase(this);
    this.Id = 0;
}
Northwind.Client.Models.Employee.prototype = {
    FirstName: null,
    FullName: null,
    LastName: null,
    PhoneExtension: 0,
    Territories: null,
    TerritoriesString: null,
    
    getEnumerator: function Northwind_Client_Models_Employee$getEnumerator() {
        /// <returns type="ss.IEnumerator"></returns>
        return (Type.safeCast(this.Territories, ss.IEnumerable)).getEnumerator();
    }
}


////////////////////////////////////////////////////////////////////////////////
// Northwind.Client.Models.EmployeeService

Northwind.Client.Models.EmployeeService = function Northwind_Client_Models_EmployeeService() {
}
Northwind.Client.Models.EmployeeService.prototype = {
    
    displayForm: function Northwind_Client_Models_EmployeeService$displayForm(employee) {
        /// <param name="employee" type="Northwind.Client.Models.Employee">
        /// </param>
        var employeeForm = $.tmpl($('#employee-form-tmpl').html(), employee);
        $('#main').append(employeeForm);
        $('#employee-form').dialog({minWidth:500,close: function(event, ui){$(this).dialog('destroy').remove();}});
    },
    
    showValidationResults: function Northwind_Client_Models_EmployeeService$showValidationResults(employee) {
        /// <param name="employee" type="Northwind.Client.Models.Employee">
        /// </param>
        var $enum1 = ss.IEnumerator.getEnumerator(employee.ValidationResultsJson);
        while ($enum1.moveNext()) {
            var validationResult = $enum1.get_current();
            if (validationResult.PropertyName === 'FirstName') {
                $('#FirstName').append('<div class=\"error-message\">' + validationResult.Message + '</div>');
            }
            if (validationResult.PropertyName === 'LastName') {
                $('#LastName').append('<div class=\"error-message\">' + validationResult.Message + '</div>');
            }
            if (validationResult.PropertyName === 'PhoneExtension') {
                $('#PhoneExtension').append('<div class=\"error-message\">' + validationResult.Message + '</div>');
            }
        }
        $('#employee-save').attr('disabled', String.Empty);
    }
}


////////////////////////////////////////////////////////////////////////////////
// Northwind.Client.Models.EmployeeViewModel

Northwind.Client.Models.EmployeeViewModel = function Northwind_Client_Models_EmployeeViewModel() {
    /// <field name="Employees" type="Array" elementType="Employee">
    /// </field>
    /// <field name="AvailableTerritories" type="Array" elementType="Territory">
    /// </field>
}
Northwind.Client.Models.EmployeeViewModel.prototype = {
    Employees: null,
    AvailableTerritories: null
}


////////////////////////////////////////////////////////////////////////////////
// Northwind.Client.Models.Territory

Northwind.Client.Models.Territory = function Northwind_Client_Models_Territory() {
    /// <field name="Description" type="String">
    /// </field>
    /// <field name="RegionBelongingTo" type="Object">
    /// </field>
    Northwind.Client.Models.Territory.initializeBase(this);
}
Northwind.Client.Models.Territory.prototype = {
    Description: null,
    RegionBelongingTo: null
}


////////////////////////////////////////////////////////////////////////////////
// Northwind.Client.Models.TerritoryService

Northwind.Client.Models.TerritoryService = function Northwind_Client_Models_TerritoryService() {
    /// <field name="employeeViewModel" type="Northwind.Client.Models.EmployeeViewModel">
    /// </field>
    this._registerClickHandlers();
}
Northwind.Client.Models.TerritoryService.prototype = {
    employeeViewModel: null,
    
    _registerClickHandlers: function Northwind_Client_Models_TerritoryService$_registerClickHandlers() {
        $('#TerritoriesAutoSuggest').live('keyup', ss.Delegate.create(this, this.territorySuggestions));
        $(document).click(ss.Delegate.create(this, function(e) {
            if (e.currentTarget.id !== 'suggestions') {
                $('#suggestions').children().remove();
            }
        }));
        $('.child-suggestion').live('click', ss.Delegate.create(this, this.addTerritoryToInput));
    },
    
    setViewModel: function Northwind_Client_Models_TerritoryService$setViewModel(viewModel) {
        /// <param name="viewModel" type="Northwind.Client.Models.EmployeeViewModel">
        /// </param>
        this.employeeViewModel = viewModel;
    },
    
    territorySuggestions: function Northwind_Client_Models_TerritoryService$territorySuggestions(eventHandler) {
        /// <param name="eventHandler" type="jQueryEvent">
        /// </param>
        eventHandler.preventDefault();
        $('#suggestions').empty();
        $('#suggestions').show();
        var territoryInput = eventHandler.currentTarget;
        var territoryArray = territoryInput.value.split(',');
        var counter = 0;
        for (var index = 0; index < this.employeeViewModel.AvailableTerritories.length; index++) {
            if (counter > 2) {
                break;
            }
            var availableTerritory = this.employeeViewModel.AvailableTerritories[index];
            var r = new RegExp(territoryArray[territoryArray.length - 1].trim(), 'i');
            if (availableTerritory.Description.search(r) !== -1) {
                $('#suggestions').append($('<div style=\"margin-left:7px;\" class=\"child-suggestion\">' + availableTerritory.Description + '</div>'));
                counter = counter + 1;
            }
        }
    },
    
    territoriestoCSV: function Northwind_Client_Models_TerritoryService$territoriestoCSV(territories) {
        /// <param name="territories" type="Array" elementType="Territory">
        /// </param>
        /// <returns type="String"></returns>
        var territoriesCommaString = new String();
        for (var index = 0; index < territories.length; index++) {
            var territory = territories[index];
            if (index === territories.length - 1) {
                territoriesCommaString = territoriesCommaString + territory.Description.trim();
            }
            else {
                territoriesCommaString = territoriesCommaString + territory.Description.trim() + ', ';
            }
        }
        return territoriesCommaString;
    },
    
    addTerritoryToInput: function Northwind_Client_Models_TerritoryService$addTerritoryToInput(eventHandler) {
        /// <param name="eventHandler" type="jQueryEvent">
        /// </param>
        var autoSuggestBox = $('#TerritoriesAutoSuggest');
        var territoryArray = autoSuggestBox.val().split(',');
        territoryArray[territoryArray.length - 1] = ' ' + eventHandler.currentTarget.innerHTML.trim() + ', ';
        $('#TerritoriesAutoSuggest').val(territoryArray.toString());
    }
}


Northwind.Client.EmployeeModule.registerClass('Northwind.Client.EmployeeModule');
Northwind.Client.EmployeeRia.registerClass('Northwind.Client.EmployeeRia');
Northwind.Client.Models.Employee.registerClass('Northwind.Client.Models.Employee', Object);
Northwind.Client.Models.EmployeeService.registerClass('Northwind.Client.Models.EmployeeService', null, Northwind.Client.Models.IEmployeeService);
Northwind.Client.Models.EmployeeViewModel.registerClass('Northwind.Client.Models.EmployeeViewModel');
Northwind.Client.Models.Territory.registerClass('Northwind.Client.Models.Territory', Object);
Northwind.Client.Models.TerritoryService.registerClass('Northwind.Client.Models.TerritoryService', null, Northwind.Client.Models.ITerritoryService);

}
ss.loader.registerScript('Northwind.Client', [], executeScript);
})();
