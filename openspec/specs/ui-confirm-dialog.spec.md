# ConfirmDialog Component Specification

## Purpose
Defines the requirements for the ConfirmDialog component used to display consistent confirmation dialogs across the application, replacing inline window.confirm() calls.

## Requirements

### Requirement: ConfirmDialog displays a title and message
The ConfirmDialog component SHALL display a title and a message when visible.

#### Scenario: Dialog with title and message
- **WHEN** the ConfirmDialog component is open and receives title and message props
- **THEN** it SHALL render the title prominently
- **AND** it SHALL render the message below the title

### Requirement: ConfirmDialog shows confirm and cancel buttons
The ConfirmDialog component SHALL show a confirm button and a cancel button.

#### Scenario: Default buttons
- **WHEN** the ConfirmDialog component is open
- **THEN** it SHALL render a confirm button labeled "Confirm" (or customizable)
- **AND** it SHALL render a cancel button labeled "Cancel" (or customizable)

#### Scenario: Custom button labels
- **WHEN** the ConfirmDialog component receives confirmLabel and cancelLabel props
- **THEN** the confirm button SHALL display the confirmLabel
- **AND** the cancel button SHALL display the cancelLabel

### Requirement: ConfirmDialog handles user interactions
The ConfirmDialog component SHALL handle user interactions with the buttons.

#### Scenario: Confirm button click
- **WHEN** the user clicks the confirm button
- **THEN** the onConfirm callback SHALL be invoked
- **AND** the dialog SHALL close (if not handled by parent)

#### Scenario: Cancel button click
- **WHEN** the user clicks the cancel button
- **THEN** the onCancel callback SHALL be invoked
- **AND** the dialog SHALL close (if not handled by parent)

#### Scenario: Close via backdrop or escape (optional)
- **WHEN** the dialog is configured to be dismissible via backdrop click or escape key
- **AND** the user clicks the backdrop or presses escape
- **THEN** the onCancel callback SHALL be invoked (or a separate onDismiss)
- **AND** the dialog SHALL close

### Requirement: ConfirmDialog controls visibility
The ConfirmDialog component SHALL be shown or hidden based on the isOpen prop.

#### Scenario: Dialog closed
- **WHEN** the ConfirmDialog component receives isOpen=false
- **THEN** it SHALL NOT render the dialog overlay or content

#### Scenario: Dialog open
- **WHEN** the ConfirmDialog component receives isOpen=true
- **THEN** it SHALL render the dialog overlay and content
- **AND** it SHALL trap focus within the dialog (accessibility consideration)

### Requirement: ConfirmDialog supports loading state on confirm
The ConfirmDialog component MAY show a loading state on the confirm button when an async operation is in progress.

#### Scenario: Loading state
- **WHEN** the ConfirmDialog component receives isConfirmLoading=true
- **THEN** the confirm button SHALL display a loading indicator
- **AND** the confirm button SHALL be disabled
- **AND** the label SHALL change to a loading message (e.g., "Deleting...") if provided