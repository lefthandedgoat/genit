module generated_bundles

open forms
open helper_general
open generated_fake_data
open generated_types
open generated_views
open generated_data_access
open generated_forms
open generated_validation

let bundle_self : Bundle<Self, SelfForm> =
    {
      validateForm = Some validation_selfForm
      convertForm = Some convert_selfForm
      fake_single = Some fake_self
      fake_many = Some fake_many_self
      tryById = Some tryById_self
      getMany = Some getMany_self
      getManyWhere = Some getManyWhere_self
      insert = Some insert_self
      update = Some update_self
      view_list = Some view_list_self
      view_edit = Some view_edit_self
      view_create = Some view_create_self
      view_generate = Some view_generate_self
      view_view = Some view_view_self
      view_search = Some view_search_self
      view_edit_errored = Some view_edit_errored_self
      view_create_errored = Some view_create_errored_self
      view_generate_errored = Some view_generate_errored_self
      href_create = "/self/create"
      href_generate = "/self/generate/%i"
      href_list = "/self/list"
      href_search = "/self/search"
      href_view = "/self/view/%i"
      href_edit = "/self/edit/%i"
    }
    
let bundle_customer : Bundle<Customer, CustomerForm> =
    {
      validateForm = Some validation_customerForm
      convertForm = Some convert_customerForm
      fake_single = Some fake_customer
      fake_many = Some fake_many_customer
      tryById = Some tryById_customer
      getMany = Some getMany_customer
      getManyWhere = None
      insert = Some insert_customer
      update = Some update_customer
      view_list = Some view_list_customer
      view_edit = Some view_edit_customer
      view_create = Some view_create_customer
      view_generate = Some view_generate_customer
      view_view = Some view_view_customer
      view_search = None
      view_edit_errored = Some view_edit_errored_customer
      view_create_errored = Some view_create_errored_customer
      view_generate_errored = Some view_generate_errored_customer
      href_create = "/customer/create"
      href_generate = "/customer/generate/%i"
      href_list = "/customer/list"
      href_search = "/customer/search"
      href_view = "/customer/view/%i"
      href_edit = "/customer/edit/%i"
    }
    
let bundle_employeeSurvey : Bundle<EmployeeSurvey, EmployeeSurveyForm> =
    {
      validateForm = Some validation_employeeSurveyForm
      convertForm = Some convert_employeeSurveyForm
      fake_single = Some fake_employeeSurvey
      fake_many = Some fake_many_employeeSurvey
      tryById = Some tryById_employeeSurvey
      getMany = Some getMany_employeeSurvey
      getManyWhere = Some getManyWhere_employeeSurvey
      insert = Some insert_employeeSurvey
      update = Some update_employeeSurvey
      view_list = Some view_list_employeeSurvey
      view_edit = Some view_edit_employeeSurvey
      view_create = Some view_create_employeeSurvey
      view_generate = Some view_generate_employeeSurvey
      view_view = Some view_view_employeeSurvey
      view_search = Some view_search_employeeSurvey
      view_edit_errored = Some view_edit_errored_employeeSurvey
      view_create_errored = Some view_create_errored_employeeSurvey
      view_generate_errored = Some view_generate_errored_employeeSurvey
      href_create = "/employeeSurvey/create"
      href_generate = "/employeeSurvey/generate/%i"
      href_list = "/employeeSurvey/list"
      href_search = "/employeeSurvey/search"
      href_view = "/employeeSurvey/view/%i"
      href_edit = "/employeeSurvey/edit/%i"
    }
    
let bundle_customerSurvey : Bundle<CustomerSurvey, CustomerSurveyForm> =
    {
      validateForm = Some validation_customerSurveyForm
      convertForm = Some convert_customerSurveyForm
      fake_single = Some fake_customerSurvey
      fake_many = Some fake_many_customerSurvey
      tryById = Some tryById_customerSurvey
      getMany = Some getMany_customerSurvey
      getManyWhere = Some getManyWhere_customerSurvey
      insert = Some insert_customerSurvey
      update = Some update_customerSurvey
      view_list = Some view_list_customerSurvey
      view_edit = Some view_edit_customerSurvey
      view_create = Some view_create_customerSurvey
      view_generate = Some view_generate_customerSurvey
      view_view = Some view_view_customerSurvey
      view_search = Some view_search_customerSurvey
      view_edit_errored = Some view_edit_errored_customerSurvey
      view_create_errored = Some view_create_errored_customerSurvey
      view_generate_errored = Some view_generate_errored_customerSurvey
      href_create = "/customerSurvey/create"
      href_generate = "/customerSurvey/generate/%i"
      href_list = "/customerSurvey/list"
      href_search = "/customerSurvey/search"
      href_view = "/customerSurvey/view/%i"
      href_edit = "/customerSurvey/edit/%i"
    }
    
  