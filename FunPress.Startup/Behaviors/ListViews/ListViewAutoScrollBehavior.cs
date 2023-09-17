using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace FunPress.Startup.Behaviors.ListViews
{
    public class ListViewAutoScrollBehavior : Behavior<ListView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Unloaded += OnUnloaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.Unloaded -= OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (AssociatedObject.ItemsSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += OnCollectionChanged;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            if (AssociatedObject.ItemsSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged -= OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            var newItem = args.NewItems[args.NewItems.Count - 1];
            AssociatedObject.ScrollIntoView(newItem);
        }
    }
}
