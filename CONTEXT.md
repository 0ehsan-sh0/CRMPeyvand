# CRMPeyvand

Persian-language CRM desktop application: manages customers, a catalog of goods and services, sales invoices, activities and reminders, SMS messaging, and employee access control.

## Language

### Sales & Catalog

**Catalog Item**:
Something the business sells — either a Good or a Service.
_Avoid_: Product (legacy name currently covering both kinds)

**Good**:
A physical Catalog Item whose Stock is tracked and reduced when sold.
_Avoid_: محصول

**Service**:
A non-physical Catalog Item; carries no Stock.

**Stock**:
The quantity on hand of a Good.

**Invoice**:
A sales document issued to a Customer listing purchased items under a human-facing number.
_Avoid_: Factor, فاکتور

**Invoice Line**:
One Catalog Item position on an Invoice, carrying its Quantity and unit price at sale time.
_Avoid_: product-invoice link, order detail

**Discount Code**:
A code that grants a discount applied to an Invoice.
_Avoid_: OffCode

### People & Access

**Customer**:
A person or organization that buys from the business.
_Avoid_: client

**User**:
An employee account that logs into the CRM.

**User Group**:
A named group of Users; Access Grants attach to the group and apply to its members.

**Section**:
A functional area of the app that access is granted on (e.g., Customers, Invoices, Users).

**Operation**:
A kind of action on a Section: View, Create, Edit, Delete.

**Access Grant**:
Permission for one User Group to perform one Operation within one Section.
_Avoid_: UserAccessRole (legacy shape)

**First Run**:
The state of the app before any User exists; on First Run the app demands creating the first User instead of showing login.
_Avoid_: Activation, License (retired legacy concepts)

### Work Tracking & Messaging

**Activity**:
A logged task or interaction concerning a Customer.

**Activity Category**:
Classification used to group Activities.

**Reminder**:
A dated follow-up alert.

**Message**:
An SMS sent to or received from a Customer.

**SMS Panel**:
A provider account through which Messages are sent.
_Avoid_: MessagePanel
