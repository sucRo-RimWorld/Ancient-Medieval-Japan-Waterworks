Feature: Add Waterworks to a saved game created without the mod

  @timeout:300
  Scenario: A Vanilla-only saved map safely accepts newly installed Waterworks
    Given the save file "waterworks-before-install" is loaded
    Then Waterworks first loads without changing Vanilla ground and can dig canals
    When I save and reload
    Then Waterworks persists and restores old Vanilla soil and gravel
